using System;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiHappinessStatController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private HappinessState _state = HappinessState.Moderate;
        [SerializeField] private Image _happinessIcon = null;
        [SerializeField] private Text _happinessText = null;

        private bool _hovered = false;

        public void Setup(HappinessState state)
        {
            _state = state;
            switch (state)
            {
                case HappinessState.Unhappy:
                    _happinessIcon.color = ColorFactoryController.Moderate;
                    break;
                case HappinessState.Moderate:
                    _happinessIcon.color = ColorFactoryController.Moderate;
                    
                    break;
                case HappinessState.Happy:
                    _happinessIcon.color = ColorFactoryController.Happiness;
                    break;
            }
            _happinessText.text = _state.ToStateString(true);
            _happinessIcon.color = _state.ToColor();
        }

        public void Clear()
        {
            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RefreshHoverInfo();
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

        private void RefreshHoverInfo()
        {
            _hovered = true;
            var showHoverinfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverinfoMsg.Title = WellBeingController.Happiness;

            var description = DescriptionFactoryController.Happiness;
            description = $"{description}{StaticMethods.DoubleNewLine()}Status:{_state.ToStateString(true)}{Environment.NewLine}";
            showHoverinfoMsg.Description = description;
            showHoverinfoMsg.Icon = WellBeingController.HappinessIcon;
            showHoverinfoMsg.Position = transform.position.ToVector2();
            showHoverinfoMsg.World = false;
            showHoverinfoMsg.Owner = gameObject;
            showHoverinfoMsg.ColorMask = _state.ToColor();
            gameObject.SendMessage(showHoverinfoMsg);
            MessageFactory.CacheMessage(showHoverinfoMsg);
        }
    }
}