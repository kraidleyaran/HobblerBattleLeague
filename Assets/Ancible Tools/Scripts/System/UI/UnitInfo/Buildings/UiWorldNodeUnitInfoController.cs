using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo.Buildings
{
    public class UiWorldNodeUnitInfoController : UiBuildingUnitInfoController
    {
        private const string FILTER = "UI_WORLD_NODE_UNIT_INFO";

        [SerializeField] private UiFillBarController _stacksFillbar;
        [SerializeField] private UiHobblerIconController _hobblerIconTemplate;
        [SerializeField] private Color _stackFillColor = Color.yellow;
        [SerializeField] private HorizontalLayoutGroup _hobblerGrid;
        [SerializeField] private RectTransform _hobblerContent;
        [SerializeField] private GameObject _hobblerGroup;

        protected internal override string _filter => FILTER;

        private Dictionary<GameObject, UiHobblerIconController> _hobblers = new Dictionary<GameObject, UiHobblerIconController>();

        private void RefreshNode(int current, int max, GameObject[] hobblers)
        {
            if (max > 0)
            {
                var percent = (float) current / max;
                _stacksFillbar.Setup(percent, $"{current}/{max}", _stackFillColor);
            }
            else
            {
                _stacksFillbar.gameObject.SetActive(false);
            }

            var removedHobblers = _hobblers.Where(kv => Array.IndexOf(hobblers, kv.Key) < 0).ToArray();
            foreach (var hobbler in removedHobblers)
            {
                if (hobbler.Value)
                {
                    hobbler.Value.Destroy();
                    Destroy(hobbler.Value.gameObject);
                }
            }

            if (hobblers.Length > 0)
            {
                var newHobblers = hobblers.Where(h => !_hobblers.ContainsKey(h)).ToArray();
                foreach (var hobbler in newHobblers)
                {
                    var controller = Instantiate(_hobblerIconTemplate, _hobblerGrid.transform);
                    controller.Setup(hobbler);
                    _hobblers.Add(hobbler, controller);
                }

                var width = Mathf.Max(_hobblers.Count * (_hobblerGrid.spacing + _hobblerIconTemplate.RectTransform.rect.width) + _hobblerGrid.padding.left, _hobblerContent.rect.width);
                _hobblerContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            _hobblerGroup.gameObject.SetActive(hobblers.Length > 0);

        }

        protected internal override void RefreshOwner()
        {
            base.RefreshOwner();

            var queryNodeMsg = MessageFactory.GenerateQueryNodeMsg();
            queryNodeMsg.DoAfter = RefreshNode;
            gameObject.SendMessageTo(queryNodeMsg, _owner);
            MessageFactory.CacheMessage(queryNodeMsg);
        }

        public override void Destroy()
        {
            var hobblers = _hobblers.ToArray();
            foreach (var hobbler in hobblers)
            {
                if (hobbler.Value)
                {
                    hobbler.Value.Destroy();
                    Destroy(hobbler.Value.gameObject);
                }
            }
            _hobblers.Clear();
            base.Destroy();
        }
    }
}