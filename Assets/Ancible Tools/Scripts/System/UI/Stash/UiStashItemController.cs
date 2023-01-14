using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash
{
    public class UiStashItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ItemStack Stack => _stack;

        [SerializeField] private Image _borderImage = null;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Text _stackText = null;

        private ItemStack _stack = null;

        private bool _hovered = false;

        public void Setup(ItemStack stack)
        {
            _stack = stack;
            _iconImage.sprite = stack.Item.Icon;
            _stackText.text = _stack.Stack > 1 ? $"x{_stack.Stack}" : string.Empty;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
            showHoverInfoMsg.Title = _stack.Item.DisplayName;
            showHoverInfoMsg.Description = _stack.Item.GetDescription();
            showHoverInfoMsg.Icon = _stack.Item.Icon;
            showHoverInfoMsg.World = false;
            showHoverInfoMsg.Owner = gameObject;
            showHoverInfoMsg.Position = transform.position.ToVector2();
            gameObject.SendMessage(showHoverInfoMsg);
            MessageFactory.CacheMessage(showHoverInfoMsg);
            _borderImage.color = ColorFactoryController.HoveredItem;

            var setHoveredStashItemMsg = MessageFactory.GenerateSetHoveredStashItemControllerMsg();
            setHoveredStashItemMsg.Controller = this;
            gameObject.SendMessage(setHoveredStashItemMsg);
            MessageFactory.CacheMessage(setHoveredStashItemMsg);
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
                _borderImage.color = Color.white;

                var removeHoveredStashItemMsg = MessageFactory.GenerateRemoveHoveredStashItemControllerMsg();
                removeHoveredStashItemMsg.Controller = this;
                gameObject.SendMessage(removeHoveredStashItemMsg);
                MessageFactory.CacheMessage(removeHoveredStashItemMsg);
            }
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
                _borderImage.color = Color.white;
            }

        }
    }
}