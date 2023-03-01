using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo.Buildings
{
    public class UiCraftingNodeUnitInfoController : UiBuildingUnitInfoController
    {
        [SerializeField] private UiQueuedCraftController _queuedCraftTemplate = null;
        [SerializeField] private HorizontalLayoutGroup _queuedGrid;
        [SerializeField] private UiHobblerIconController _hobblerIconTemplate;
        [SerializeField] private HorizontalLayoutGroup _hobblerGrid;
        [SerializeField] private RectTransform _hobblerContent;
        [SerializeField] private GameObject _hobblerGroup;

        private List<UiQueuedCraftController> _controllers = new List<UiQueuedCraftController>();

        private Dictionary<GameObject, UiHobblerIconController> _hobblers = new Dictionary<GameObject, UiHobblerIconController>();

        protected internal override void RefreshOwner()
        {
            base.RefreshOwner();

            var queryCraftingQueueMsg = MessageFactory.GenerateQueryCraftingQueueMsg();
            queryCraftingQueueMsg.DoAfter = RefreshCraftingQueue;
            gameObject.SendMessageTo(queryCraftingQueueMsg, _owner);
            MessageFactory.CacheMessage(queryCraftingQueueMsg);

            var queryNodeMsg = MessageFactory.GenerateQueryNodeMsg();
            queryNodeMsg.DoAfter = RefreshNode;
            gameObject.SendMessageTo(queryNodeMsg, _owner);
            MessageFactory.CacheMessage(queryNodeMsg);
        }

        private void RefreshCraftingQueue(QueuedCraft[] queue, int max)
        {
            if (_controllers.Count < max)
            {
                for (var i = _controllers.Count; i < max; i++)
                {
                    var controller = Instantiate(_queuedCraftTemplate, _queuedGrid.transform);
                    controller.SetOwner(_owner);
                    _controllers.Add(controller);
                    controller.Clear();
                }
            }

            for (var i = 0; i < max; i++)
            {
                var controller = _controllers[i];
                controller.SetIndex(i, queue.Length);
                if (i < queue.Length)
                {
                    if (controller.Craft == null || controller.Craft != queue[i])
                    {
                        controller.Setup(queue[i]);
                    }
                    else
                    {
                        controller.RefreshCraft();
                    }
                }
                else if (controller.Craft != null)
                {
                    controller.Clear();
                }
            }
        }

        private void RefreshNode(int current, int max, GameObject[] hobblers)
        {
            var removedHobblers = _hobblers.Where(kv => Array.IndexOf(hobblers, kv.Key) < 0).ToArray();
            foreach (var hobbler in removedHobblers)
            {
                _hobblers.Remove(hobbler.Key);
                hobbler.Value.Destroy();
                Destroy(hobbler.Value.gameObject);
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

                var width = _hobblers.Count * (_hobblerGrid.spacing + _hobblerIconTemplate.RectTransform.rect.width) + _hobblerGrid.padding.left;
                _hobblerContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            _hobblerGroup.gameObject.SetActive(hobblers.Length > 0);
        }

        public override void Destroy()
        {
            var hobblers = _hobblers.ToArray();
            foreach (var hobbler in hobblers)
            {
                hobbler.Value.Destroy();
                Destroy(hobbler.Value.gameObject);
            }
            _hobblers.Clear();
            base.Destroy();
        }
    }
}