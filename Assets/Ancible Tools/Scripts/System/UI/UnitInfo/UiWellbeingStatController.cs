using System;
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

        private int _stat = 0;
        private int _min = 0;
        private int _max = 0;

        private bool _hovered = false;

        public void Setup(int stat, int min, int max)
        {
            _stat = stat * -1;
            _min = min;
            _max = max;
            if (_stat > 0)
            {
                var fillMax = _min * -1;
                var percent = (float) _stat / fillMax;
                _fillBarController.Setup(percent, $"{_stat}", _fillColor);
            }
            else if (_stat == 0)
            {
                _fillBarController.Setup(0f, $"{_stat}", _fillColor);
            }
            else
            {
                var percent = (float) stat / max;
                _fillBarController.Setup(percent, $"{_stat}", ColorFactoryController.NegativeStatColor);
            }
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
            description = $"{description}{Environment.NewLine}{Environment.NewLine}{_stat}/{_max}";
            showHoverInfoMsg.Description = description;
            showHoverInfoMsg.Icon = _iconImage.sprite;
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