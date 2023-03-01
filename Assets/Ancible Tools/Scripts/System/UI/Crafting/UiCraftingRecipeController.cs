using System;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
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
        [SerializeField] private Image _qualityIcon;
        [SerializeField] private Text _abilityRankText = null;


        private bool _hovered = false;
        private bool _selected = false;

        private GameObject _parent = null;

        public void Setup(CraftingRecipe recipe, GameObject parent)
        {
            Recipe = recipe;
            _parent = parent;
            _iconImage.sprite = Recipe.Item.Item.Icon;
            if (recipe.Item.Item.Quality != ItemQuality.Basic)
            {
                switch (recipe.Item.Item.Quality)
                {
                    case ItemQuality.Improved:
                        _qualityIcon.sprite = IconFactoryController.ImprovedIcon;
                        break;
                    case ItemQuality.Ornate:
                        _qualityIcon.sprite = IconFactoryController.OrnateIcon;
                        break;
                }
            }

            if (recipe.Item.Item.Type == WorldItemType.Ability && recipe.Item.Item is AbilityItem abilityItem && abilityItem.Ability.Rank > 0)
            {
                _abilityRankText.text = StaticMethods.ApplyColorToText(abilityItem.Ability.RankToString(), ColorFactoryController.AbilityRank);
            }
            else
            {
                _abilityRankText.gameObject.SetActive(false);
            }
            _qualityIcon.gameObject.SetActive(recipe.Item.Item.Quality != ItemQuality.Basic);
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
                var description = $"{Recipe.Item.Item.GetDescription()}{Environment.NewLine}{Environment.NewLine}Time:{Recipe.CraftingTicks}";
                showHoverInfoMsg.Description = description;
                gameObject.SendMessage(showHoverInfoMsg);
                MessageFactory.CacheMessage(showHoverInfoMsg);

                _borderImage.color = ColorFactoryController.HoveredItem;
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