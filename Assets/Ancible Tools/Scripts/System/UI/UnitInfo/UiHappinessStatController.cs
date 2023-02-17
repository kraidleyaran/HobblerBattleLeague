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
        [SerializeField] private UiFillBarController _fillBarController;
        [SerializeField] [Range(0f, 1f)] private float _defaultNegativePercent = .05f;

        private float _percent;
        private HappinessState _state = HappinessState.Moderate;

        private bool _hovered = false;

        public void Setup(float percent, HappinessState state)
        {
            _state = state;
            _percent = percent;
            var barPercent = Mathf.Max(_defaultNegativePercent, _percent);
            _fillBarController.Setup(barPercent, $"{(int)(_percent * 100f)}/100", _state.ToColor());
            if (_hovered)
            {
                RefreshHoverInfo();
            }
        }

        public void Clear()
        {
            _fillBarController.Clear();
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
            description = $"{description}{StaticMethods.DoubleNewLine()}Happiness: {(int)(_percent * 100f)}/100{StaticMethods.DoubleNewLine()}Status: {_state.ToStateString(true)}{Environment.NewLine}";
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