using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiCraftingRecipeController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public CraftingRecipe Recipe { get; private set; }

        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _borderImage;

        private bool _hovered = false;
        private bool _selected = false;

        private GameObject _parent = null;

        public void Setup(CraftingRecipe recipe, GameObject parent)
        {
            Recipe = recipe;
            _parent = parent;
            _iconImage.sprite = Recipe.Item.Item.Icon;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered)
            {
                _hovered = true;
                var showHoverInfoMsg = MessageFactory.GenerateShowHoverInfoMsg();
                showHoverInfoMsg.Owner = gameObject;
                showHoverInfoMsg.Icon = Recipe.Item.Item.Icon;
                showHoverInfoMsg.Gold = Recipe.Cost;
                showHoverInfoMsg.Title = Recipe.Item.Item.DisplayName;
                showHoverInfoMsg.Description = Recipe.Item.Item.GetDescription();
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);

                _borderImage.color = ColorFactoryController.HoveredItem;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hovered)
            {
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);

                _borderImage.color = _selected ? ColorFactoryController.SelectedItem : Color.white;
            }
        }

        public void Click()
        {
            if (!_selected)
            {
                _selected = true;
                _borderImage.color = _hovered ? ColorFactoryController.HoveredItem : ColorFactoryController.SelectedItem;
                var setSelectedCraftingRecipeMsg = MessageFactory.GenerateSetSelectedCraftingRecipeMsg();
                setSelectedCraftingRecipeMsg.Controller = this;
                gameObject.SendMessageTo(setSelectedCraftingRecipeMsg, _parent);
                MessageFactory.CacheMessage(setSelectedCraftingRecipeMsg);
            }
        }

        public void Unselect()
        {
            if (_selected)
            {
                _selected = false;
                _borderImage.color = _selected ? ColorFactoryController.SelectedItem : Color.white;
            }
        }

        public void Destroy()
        {
            if (_hovered)
            {
                var removeHoverInfoMsg = MessageFactory.GenerateRemoveHoverInfoMsg();
                removeHoverInfoMsg.Owner = gameObject;
                gameObject.SendMessage(removeHoverInfoMsg);
                MessageFactory.CacheMessage(removeHoverInfoMsg);
            }
        }

        void OnDestroy()
        {
            Destroy();
        }
    }
}