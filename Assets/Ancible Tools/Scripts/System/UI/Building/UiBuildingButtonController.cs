﻿using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Building
{
    public class UiBuildingButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _frameImage = null;

        public WorldBuilding Building { get; private set; }

        private bool _hovered = false;

        public void Setup(WorldBuilding building)
        {
            Building = building;
            _iconImage.sprite = Building.Icon;
        }

        public void Click()
        {
            WorldBuildingManager.SetupBuilding(Building);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                _frameImage.color = ColorFactoryController.HoveredItem;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Position = transform.position.ToVector2();
                showHoverInfoMsg.Description = Building.Description;
                showHoverInfoMsg.Title = Building.DisplayName;
                showHoverInfoMsg.Icon = Building.Icon;
                showHoverInfoMsg.Owner = gameObject;
                showHoverInfoMsg.World = false;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                _hovered = false;
                _frameImage.color = Color.white;
                var removeHoveredInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoveredInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoveredInfoMsg);
                MessageFactory.CacheMessage(removeHoveredInfoMsg);
            }
        }

        void OnDisable()
        {
            _frameImage.color = Color.white;
        }

        public void Destroy()
        {
            if (_hovered)
            {
                var removeHoveredInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoveredInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoveredInfoMsg);
                MessageFactory.CacheMessage(removeHoveredInfoMsg);
            }
        }
    }
}