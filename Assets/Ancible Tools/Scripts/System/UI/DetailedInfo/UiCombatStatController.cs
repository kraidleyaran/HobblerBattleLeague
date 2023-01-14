using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo
{
    public class UiCombatStatController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _text;
        [SerializeField] private CombatStatType _type;

        private int _stat = 0;
        private int _bonus = 0;

        private bool _hovered = false;

        public void Setup(int stat, int bonus = 0)
        {
            _stat = stat;
            _bonus = bonus;
            _text.text = GetStatText();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoveredInfoMsg.Title = $"{_type}";
            var description = DescriptionFactoryController.GetCombatStatDescription(_type);
            description = $"{description}{Environment.NewLine}{Environment.NewLine}";
            var statDesciprtion = GetStatText();
            description = $"{description}{statDesciprtion}";
            if (_bonus > 0)
            {
                description = $"{description} = {_stat + _bonus}";
            }
            //description = $"{description}{Environment.NewLine}{GetStatText()}";
            showHoveredInfoMsg.Description = description;
            showHoveredInfoMsg.Icon = _iconImage.sprite;
            showHoveredInfoMsg.Position = _iconImage.transform.position.ToVector2();
            showHoveredInfoMsg.World = false;
            showHoveredInfoMsg.Owner = gameObject;
            gameObject.SendMessage(showHoveredInfoMsg);
            MessageFactory.CacheMessage(showHoveredInfoMsg);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            var removeHoveredInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
            removeHoveredInfoMsg.Owner = gameObject;
            gameObject.SendMessage(removeHoveredInfoMsg);
            MessageFactory.CacheMessage(removeHoveredInfoMsg);
        }

        private string GetStatText()
        {
            return _bonus > 0 ? $"{_stat}{StaticMethods.ApplyColorToText($"+{_bonus}", ColorFactoryController.BonusStat)}" : $"{_stat}";
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                _hovered = false;
                var removeHoveredInfoMsg = MessageFactory.GenerateRemoveHoveredUnitMsg();
                removeHoveredInfoMsg.Unit = gameObject;
                gameObject.SendMessage(removeHoveredInfoMsg);
                MessageFactory.CacheMessage(removeHoveredInfoMsg);
            }
        }
    }
}