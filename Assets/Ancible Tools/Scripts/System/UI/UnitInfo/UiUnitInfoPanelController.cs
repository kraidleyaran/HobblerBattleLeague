﻿using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiUnitInfoPanelController : UiBaseWindow
    {
        private const string FILTER = "UI_INFO_PANEL";

        private static UiUnitInfoPanelController _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Image _unitSprite;
        [SerializeField] private UiCommandCardController _commandCardController = null;
        [Space]
        [Header("Info Templates")]
        [SerializeField] private UiHobblerInfoController _hobblerInfoTemplate;

        private GameObject _selectedUnit = null;
        private GameObject _infoController = null;
        private QuerySpriteMessage _querySpriteMsg = new QuerySpriteMessage();
        private QueryWorldUnitTypeMessage _queryWorldUnitTypeMsg = new QueryWorldUnitTypeMessage();

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            _querySpriteMsg.DoAfter = RefreshSprite;
            _queryWorldUnitTypeMsg.DoAfter = RefreshWorldUnitType;
            Clear();
            _commandCardController.Setup();
            SubscribeToMessages();
            base.Awake();
        }

        private void Clear()
        {
            _unitSprite.sprite = null;
            gameObject.SetActive(false);
        }

        private void RefreshSprite(SpriteTrait sprite)
        {
            if (sprite)
            {
                _unitSprite.sprite = sprite.Sprite;
            }
            _unitSprite.gameObject.SetActive(_unitSprite.sprite);
        }

        private void RefreshWorldUnitType(WorldUnitType type)
        {
            if (_infoController)
            {
                Destroy(_infoController);
                _infoController = null;
            }

            if (_selectedUnit)
            {
                switch (type)
                {
                    case WorldUnitType.Hobbler:
                        var hobblerInfo = Instantiate(_hobblerInfoTemplate, transform);
                        hobblerInfo.Setup(_selectedUnit);
                        _infoController = hobblerInfo.gameObject;
                        break;
                    case WorldUnitType.Interactable:
                        var queryBuildingMsg = MessageFactory.GenerateQueryBuildingMsg();
                        queryBuildingMsg.DoAfter = SetupBuilding;
                        gameObject.SendMessageTo(queryBuildingMsg, _selectedUnit);
                        MessageFactory.CacheMessage(queryBuildingMsg);
                        break;
                }
            }

        }

        private void RefreshInfo()
        {
            if (_selectedUnit)
            {
                gameObject.SendMessageTo(_querySpriteMsg, _selectedUnit);
                gameObject.SetActive(true);
            }
            else
            {
                if (_infoController)
                {
                    Destroy(_infoController);
                    _infoController = null;
                }
                if (_hovered)
                {
                    UiWindowManager.RemoveHoveredWindow(this);
                }
                Clear();
            }
            
        }

        private void SetupBuilding(WorldBuilding building, MapTile tile, string id)
        {
            if (building.UnitInfoTemplate)
            {
                var buildingInfo = Instantiate(building.UnitInfoTemplate, transform);
                buildingInfo.Setup(_selectedUnit, building);
                _infoController = buildingInfo.gameObject;
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateSelectedUnitMessage>(UpdateSelectedUnit);
        }

        private void UpdateSelectedUnit(UpdateSelectedUnitMessage msg)
        {
            if (msg.Unit)
            {
                if (!_selectedUnit || _selectedUnit != msg.Unit)
                {
                    if (_selectedUnit)
                    {
                        _selectedUnit.UnsubscribeFromAllMessagesWithFilter(FILTER);
                    }
                    _selectedUnit = msg.Unit;
                    gameObject.SendMessageTo(_queryWorldUnitTypeMsg, _selectedUnit);
                    _selectedUnit.gameObject.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, FILTER);
                    RefreshInfo();
                }
            }
            else if (_selectedUnit)
            {
                _selectedUnit.UnsubscribeFromAllMessagesWithFilter(FILTER);
                _selectedUnit = null;
                RefreshInfo();
            }
            

        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshInfo();
        }

        protected internal override void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            if (_selectedUnit || DisabledStates.Contains(msg.State))
            {
                base.UpdateWorldState(msg);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public override void Destroy()
        {
            if (_selectedUnit)
            {
                _selectedUnit.UnsubscribeFromAllMessagesWithFilter(FILTER);
            }
            base.Destroy();
        }
    }
}