using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HobblerGenerator
{
    public class UiHobblerWeaponController : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] private Image _weaponImage;

        private WeaponItem _weapon = null;
        private bool _hovered = false;

        public void Setup(WeaponItem item)
        {
            _weapon = item;
            _weaponImage.sprite = item.Icon;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Title = _weapon.DisplayName;
                showHoverInfoMsg.Description = _weapon.GetDescription();
                showHoverInfoMsg.Icon = _weapon.Icon;
                showHoverInfoMsg.World = false;
                showHoverInfoMsg.Position = transform.position.ToVector2();
                showHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);
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
    }
}