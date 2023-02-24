using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo
{
    public class UiEquippedItemController : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
    {
        public EquippableItem Item => _equipped;
        public EquipSlot Slot => _equipSlot;

        [SerializeField] private Image _emptyIcon;
        [SerializeField] private Image _equippedIcon;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _qualityIcon;
        [SerializeField] private EquipSlot _equipSlot = EquipSlot.Armor;
        

        private EquippableItem _equipped = null;

        private bool _hovered = false;
        private GameObject _parent = null;

        public void Setup(EquippableItem item, GameObject parent)
        {
            _equipped = item;
            _parent = parent;
            if (_equipped)
            {
                _equippedIcon.sprite = _equipped.Icon;
                _emptyIcon.gameObject.SetActive(false);
                _equippedIcon.gameObject.SetActive(true);
                _borderImage.color = _equipped.Rarity.ToRarityColor();
                _qualityIcon.sprite = _equipped.Quality.ToIcon();
                _qualityIcon.gameObject.SetActive(_equipped.Quality != ItemQuality.Basic);
            }
            else
            {
                _emptyIcon.gameObject.SetActive(true);
                _equippedIcon.gameObject.SetActive(false);
                _qualityIcon.gameObject.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoveredInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoveredInfoMsg.Icon = _equipped ? _equipped.Icon : _emptyIcon.sprite;
            showHoveredInfoMsg.Title = _equipped ? _equipped.DisplayName : $"{_equipSlot}";
            showHoveredInfoMsg.Description = _equipped ? _equipped.GetDescription() : string.Empty;
            showHoveredInfoMsg.Owner = gameObject;
            showHoveredInfoMsg.World = false;
            showHoveredInfoMsg.Position = transform.position.ToVector2();
            showHoveredInfoMsg.Gold = _equipped ? _equipped.GoldValue : -1;
            gameObject.SendMessage(showHoveredInfoMsg);
            MessageFactory.CacheMessage(showHoveredInfoMsg);
            _borderImage.color = ColorFactoryController.HoveredItem;
            var setHoveredEquippedItemControllerMsg = MessageFactory.GenerateSetHoveredEquippedItemControllerMsg();
            setHoveredEquippedItemControllerMsg.Controller = this;
            gameObject.SendMessageTo(setHoveredEquippedItemControllerMsg, _parent);
            MessageFactory.CacheMessage(setHoveredEquippedItemControllerMsg);
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
                _borderImage.color = _equipped ? _equipped.Rarity.ToRarityColor() : Color.white;
                var removeHoveredEquippedItemControllerMsg = MessageFactory.GenerateRemoveHoveredEquippedItemControllerMsg();
                removeHoveredEquippedItemControllerMsg.Controller = this;
                gameObject.SendMessageTo(removeHoveredEquippedItemControllerMsg, _parent);
                MessageFactory.CacheMessage(removeHoveredEquippedItemControllerMsg);

            }
        }

        void OnDestroy()
        {
            if (_hovered)
            {
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);

                if (_parent)
                {
                    var removeHoveredEquippedItemControllerMsg = MessageFactory.GenerateRemoveHoveredEquippedItemControllerMsg();
                    removeHoveredEquippedItemControllerMsg.Controller = this;
                    gameObject.SendMessageTo(removeHoveredEquippedItemControllerMsg, _parent);
                    MessageFactory.CacheMessage(removeHoveredEquippedItemControllerMsg);
                }
            }
        }
    }
}