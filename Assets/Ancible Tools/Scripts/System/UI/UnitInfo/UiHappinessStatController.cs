using System;
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
        [SerializeField] private Color _fillColor = Color.white;
        [SerializeField] [Range(0f, 1f)] private float _defaultNegativePercent = .05f;

        private int _stat = 0;
        private int _min = 0;
        private int _max = 0;

        private bool _hovered = false;

        public void Setup(int stat, int min, int max)
        {
            _stat = stat;
            _min = min;
            _max = max;
            if (_stat < 0)
            {
                _fillBarController.Setup(_defaultNegativePercent, $"{_stat}", ColorFactoryController.NegativeStatColor);
            }
            else if (_stat == 0)
            {
                _fillBarController.Setup(0f, $"{_stat}", _fillColor);
            }
            else
            {
                var percent = (float)stat / _max;
                _fillBarController.Setup(percent, $"{_stat}", _fillColor);
            }
        }

        public void Clear()
        {
            _fillBarController.Clear();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoverinfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverinfoMsg.Title = WellBeingController.Happiness;
            var description = DescriptionFactoryController.Happiness;
            description = $"{description}{Environment.NewLine}{Environment.NewLine}{_stat} / {_max}";
            showHoverinfoMsg.Description = description;
            showHoverinfoMsg.Icon = WellBeingController.HappinessIcon;
            showHoverinfoMsg.Position = transform.position.ToVector2();
            showHoverinfoMsg.World = false;
            showHoverinfoMsg.Owner = gameObject;
            gameObject.SendMessage(showHoverinfoMsg);
            MessageFactory.CacheMessage(showHoverinfoMsg);
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
    }
}