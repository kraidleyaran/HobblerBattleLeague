using System;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.UI.UnitInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DragDrop;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo
{
    public class UiDetailedHobblerInfoWindowController : UiBaseWindow
    {
        public const string FILTER = "UI_DETAILED_HOBBLER_INFO_WINDOW";

        [SerializeField] private Text _nameText = null;
        [SerializeField] private Image _frameImage = null;

        [SerializeField] private UiHobblerEquipmentInfoController _equipmentController = null;
        [SerializeField] private Color _equipmentInfoFrameColor = Color.white;
        [SerializeField] private UiHobblerAbilityInfoController _abilityInfoController = null;
        [SerializeField] private Color _abilityInfoFrameColor = Color.blue;
        [SerializeField] private UiHobblerSkillManager _skillManager = null;
        [SerializeField] private Color _skillManagerColor = Color.cyan;

        private GameObject _unit = null;
        private GameObject _activeInfo = null;

        private HobblerInfoState _state = HobblerInfoState.Equipment;

        public void Setup(GameObject obj)
        {
            _unit = obj;
            _equipmentController.Setup(_unit);
            _equipmentController.gameObject.SetActive(false);
            _abilityInfoController.Setup(_unit);
            _abilityInfoController.gameObject.SetActive(false);
            _skillManager.Setup(_unit);
            _skillManager.gameObject.SetActive(false);
            SetHobblerInfoState(0);
            RefreshInfo();
            SubscribeToMessages();
        }

        public void SetHobblerInfoState(int value)
        {
            var state = (HobblerInfoState) value;
            if (_activeInfo)
            {
                _activeInfo.gameObject.SetActive(false);
            }
            var frameColor = _equipmentInfoFrameColor;
            switch (state)
            {
                case HobblerInfoState.Equipment:
                    if (_activeInfo != _equipmentController.gameObject)
                    {
                        _activeInfo = _equipmentController.gameObject;
                        _equipmentController.RefreshInfo();
                    }

                    break;
                case HobblerInfoState.Abilities:
                    frameColor = _abilityInfoFrameColor;
                    if (_activeInfo != _abilityInfoController.gameObject)
                    {
                        _activeInfo = _abilityInfoController.gameObject;
                        _abilityInfoController.RefreshInfo();
                    }
                    break;
                case HobblerInfoState.Talents:
                    break;
                case HobblerInfoState.Skills:
                    frameColor = _skillManagerColor;
                    if (_activeInfo != _skillManager.gameObject)
                    {
                        _activeInfo = _skillManager.gameObject;
                        _skillManager.RefreshInfo();
                    }
                    break;
            }
            _activeInfo.gameObject.SetActive(true);
            _frameImage.color = frameColor;
        }

        private void RefreshInfo()
        {
            var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryNameMsg.DoAfter = RefreshName;
            gameObject.SendMessageTo(queryNameMsg, _unit);
            MessageFactory.CacheMessage(queryNameMsg);
        }

        private void RefreshName(string unitName)
        {
            _nameText.text = $"{unitName}";
        }

        private void SubscribeToMessages()
        {
            gameObject.SubscribeWithFilter<ReceiveDragDropItemMessage>(ReceiveDragDropItem, FILTER);
            gameObject.SubscribeWithFilter<RemoveItemMessage>(RemoveItem, FILTER);
            gameObject.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
        }

        private void ReceiveDragDropItem(ReceiveDragDropItemMessage msg)
        {
            gameObject.SendMessageTo(msg, _activeInfo);
        }

        private void RemoveItem(RemoveItemMessage msg)
        {
            gameObject.SendMessageTo(msg, _activeInfo);
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {

        }

        public override void Destroy()
        {
            _abilityInfoController.Destroy();
            _equipmentController.Destroy();
            _skillManager.Destroy();
            base.Destroy();
        }
    }
}