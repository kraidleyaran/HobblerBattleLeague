﻿using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiHobblerIconController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const string FILTER = "UI_HOBBLER_ICON";

        public GameObject Hobbler { get; private set; }

        public RectTransform RectTransform;
        public Button Button;
        [SerializeField] private Image _iconImage;

        private SpriteTrait _sprite = null;
        private string _name = string.Empty;
        private string _filter = string.Empty;
        private bool _hovered = false;

        private HobblerTemplate _template = null;

        public void Setup(GameObject hobbler)
        {
            Hobbler = hobbler;
            _filter = $"{FILTER}{GetInstanceID()}";
            Hobbler.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, _filter);
            RefreshOwner();
        }

        public void Setup(HobblerTemplate template)
        {
            _template = template;
            _iconImage.sprite = _template.Sprite.Sprite;
        }

        private void RefreshOwner()
        {
            var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
            querySpriteMsg.DoAfter = RefreshSprite;
            gameObject.SendMessageTo(querySpriteMsg, Hobbler);
            MessageFactory.CacheMessage(querySpriteMsg);

            var queryNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryNameMsg.DoAfter = RefreshName;
            gameObject.SendMessageTo(queryNameMsg, Hobbler);
            MessageFactory.CacheMessage(queryNameMsg);
        }

        private void RefreshSprite(SpriteTrait sprite)
        {
            _sprite = sprite;
            _iconImage.sprite = sprite.Sprite;
        }

        private void RefreshName(string hobblerName)
        {
            _name = hobblerName;
        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshOwner();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                if (Hobbler)
                {
                    showHoveredInfoMsg.Title = _name;
                    showHoveredInfoMsg.Icon = _sprite.Sprite;
                    showHoveredInfoMsg.Owner = gameObject;
                    showHoveredInfoMsg.Description = "Click to select this Hobbler";
                    
                }
                else
                {
                    showHoveredInfoMsg.Title = _template.DisplayName;
                    showHoveredInfoMsg.Description = _template.GetDescription();
                    showHoveredInfoMsg.Icon = _template.Sprite.Sprite;
                    showHoveredInfoMsg.Owner = gameObject;
                    showHoveredInfoMsg.Gold = _template.Cost;
                    
                }
                gameObject.SendMessage(showHoveredInfoMsg);
                MessageFactory.CacheMessage(showHoveredInfoMsg);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }

        public void SelectHobbler()
        {
            UnitSelectController.SelectUnit(Hobbler);
        }

        public void Destroy()
        {
            Hobbler.UnsubscribeFromAllMessagesWithFilter(_filter);
            if (_hovered)
            {
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }

            if (Button)
            {
                Button.onClick.RemoveAllListeners();
            }
            _sprite = null;
            _name = null;
        }

    }
}