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
    public class UiWellbeingStatController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private WellbeingStatType _statType;
        [SerializeField] private UiFillBarController _fillBarController;
        [SerializeField] private Color _fillColor = Color.white;
        [SerializeField] [Range(0f, 1f)] private float _minimumPercent = .05f;

        private float _stat = 0;
        private float _max = 0;

        private bool _hovered = false;

        public void Setup(float stat, float max)
        {
            _stat = stat;
            _max = max;
            var percent = (max - stat) / max;
            var displayStat = (int) (percent * 100f);
            if (percent > 0f)
            {
                if (displayStat < 1)
                {
                    displayStat = 1;
                }
                percent = Mathf.Max(_minimumPercent, percent);
            }
            _fillBarController.Setup(percent, $"{displayStat}/100", _fillColor);
        }

        public void Clear()
        {
            _fillBarController.Clear();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverInfoMsg.Title = WellBeingController.GetPositiveStatName(_statType);
            var description = DescriptionFactoryController.GetWellbeingDescription(_statType);
            var percent = ((float) _max - _stat) / _max;
            var displayStat = (int) (percent * 100f);
            if (percent > 0f && displayStat < 1)
            {
                displayStat = 1;
            }
            description = $"{description}{StaticMethods.DoubleNewLine()}{displayStat}/100{Environment.NewLine}";
            showHoverInfoMsg.Description = description;
            showHoverInfoMsg.Icon = _iconImage.sprite;
            showHoverInfoMsg.ColorMask = _iconImage.color;
            showHoverInfoMsg.World = false;
            showHoverInfoMsg.Position = transform.position.ToVector2();
            showHoverInfoMsg.Owner = gameObject;
            gameObject.SendMessage(showHoverInfoMsg);
            MessageFactory.CacheMessage(showHoverInfoMsg);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
            removeHoverInfoMsg.Owner = gameObject;
            gameObject.SendMessage(removeHoverInfoMsg);
            MessageFactory.CacheMessage(removeHoverInfoMsg);
        }

        void OnDestroy()
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