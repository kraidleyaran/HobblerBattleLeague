using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.StatusBar
{
    public class UiHobblerStatusBarController : MonoBehaviour
    {
        private const string UI_HOBBLER_STATUS_BAR_FILTER = "UI_HOBBLER_STATUS_BAR";

        [SerializeField] private Transform _iconTransform;
        [SerializeField] private Text _nameText;

        private GameObject _obj;
        private Vector2 _offset = Vector2.zero;

        private Dictionary<WellbeingStatType, UiHobblerStatusIconController> _iconControllers = new Dictionary<WellbeingStatType, UiHobblerStatusIconController>();

        public void Setup(GameObject obj, Vector2 offset)
        {
            _obj = obj;
            _offset = offset;
            _obj.gameObject.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, UI_HOBBLER_STATUS_BAR_FILTER);
            RefreshInfo();
            SubscribeToMessages();
        }

        private void RefreshInfo()
        {
            var queryWellbeingStatusMsg = MessageFactory.GenerateQueryHobblerWellbeingStatusMsg();
            queryWellbeingStatusMsg.DoAfter = RefreshStatusIcons;
            gameObject.SendMessageTo(queryWellbeingStatusMsg, _obj);
            MessageFactory.CacheMessage(queryWellbeingStatusMsg);

            var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryNameMsg.DoAfter = RefreshName;
            gameObject.SendMessageTo(queryNameMsg, _obj);
            MessageFactory.CacheMessage(queryNameMsg);
        }

        private void RefreshName(string hobblerName)
        {
            _nameText.text = hobblerName;
        }

        private void RefreshStatusIcons(WellbeingStatType[] types)
        {
            var removedTypes = _iconControllers.Keys.Where(k => !types.Contains(k)).ToArray();
            for (var i = 0; i < removedTypes.Length; i++)
            {
                var obj = _iconControllers[removedTypes[i]].gameObject;
                _iconControllers.Remove(removedTypes[i]);
                Destroy(obj);
            }

            var addTypes = types.Where(t => !_iconControllers.Keys.Contains(t)).ToArray();
            for (var i = 0; i < addTypes.Length; i++)
            {
                var icon = UiHobblerStatusBarManager.GetStatusIcon(addTypes[i]);
                if (icon != null)
                {
                    var controller = Instantiate(UiHobblerStatusBarManager.IconTemplate, _iconTransform);
                    controller.Setup(icon);
                    _iconControllers.Add(addTypes[i], controller);
                }
            }

            var currentTypes = _iconControllers.Keys.OrderBy(k => k).ToArray();
            for (var i = 0; i < currentTypes.Length; i++)
            {
                _iconControllers[currentTypes[i]].transform.SetSiblingIndex(i);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            var pos = WorldCameraController.Camera.WorldToScreenPoint(_obj.transform.position.ToVector2() +_offset);
            transform.SetTransformPosition(pos);
        }


        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        

        public void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
            if (_obj)
            {
                _obj.UnsubscribeFromAllMessagesWithFilter(UI_HOBBLER_STATUS_BAR_FILTER);
            }
            _obj = null;
            _offset = Vector2.zero;
        }
    }
}