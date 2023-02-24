using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiCraftingRecipeManager : MonoBehaviour
    {
        private const string FILTER = "UI_CRAFTING_RECIPE_MANAGER";

        [SerializeField] private UiCraftingRecipeController _craftingRecipeTemplate;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private RectTransform _content;

        private Dictionary<CraftingRecipe, UiCraftingRecipeController> _controllers = new Dictionary<CraftingRecipe, UiCraftingRecipeController>();

        private UiCraftingRecipeController _selectedController = null;

        private GameObject _parent = null;
        private GameObject _owner = null;
        private string _filter = string.Empty;

        public void Setup(GameObject owner, GameObject parent)
        {
            _owner = owner;
            _parent = parent;
            _filter = $"{FILTER}{_parent.GetInstanceID()}";
            RefreshOwner();
            SubscribeToMessages();
        }

        private void RefreshOwner()
        {
            var queryCraftingRecipesMsg = MessageFactory.GenerateQueryCraftingRecipesMsg();
            queryCraftingRecipesMsg.DoAfter = RefreshRecipes;
            gameObject.SendMessageTo(queryCraftingRecipesMsg, _owner);
            MessageFactory.CacheMessage(queryCraftingRecipesMsg);
        }

        private void RefreshRecipes(CraftingRecipe[] recipes)
        {
            var orderedRecipes = recipes.OrderBy(r => r.Item.Item.DisplayName).ThenBy(r => r.Item.Item.Quality).ToArray();
            foreach (var recipe in orderedRecipes)
            {
                if (!_controllers.ContainsKey(recipe))
                {
                    var controller = Instantiate(_craftingRecipeTemplate, _grid.transform);
                    controller.Setup(recipe, gameObject);
                    _controllers.Add(recipe, controller);
                }

            }

            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                controller.transform.SetSiblingIndex(Array.IndexOf(orderedRecipes, controller.Recipe));
            }

            var rows = _controllers.Count / _grid.constraintCount;
            var rowCheck = rows * _grid.constraintCount;
            if (rowCheck < _controllers.Count)
            {
                rows++;
            }

            var height = rows * (_grid.cellSize.y + _grid.spacing.y) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            
        }

        private void SubscribeToMessages()
        {
            _owner.SubscribeWithFilter<RefreshUnitMessage>(RefreshUnit, _filter);
            gameObject.SubscribeWithFilter<SetSelectedCraftingRecipeControllerMessage>(SetSelectedCraftingRecipe, _filter);
        }

        private void SetSelectedCraftingRecipe(SetSelectedCraftingRecipeControllerMessage msg)
        {
            if (!_selectedController || _selectedController != msg.Controller)
            {
                if (_selectedController)
                {
                    _selectedController.Unselect();
                }

                _selectedController = msg.Controller;
                gameObject.SendMessageTo(msg, _parent);
            }

        }

        private void RefreshUnit(RefreshUnitMessage msg)
        {
            RefreshOwner();
        }

        public void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
            if (_owner)
            {
                _owner.UnsubscribeFromAllMessagesWithFilter(_filter);
            }
            _filter = null;
            _owner = null;
            _parent = null;
            var controllers = _controllers.Values.ToArray();
            foreach (var controller in controllers)
            {
                controller.Destroy();
                Destroy(controller.gameObject);
            }
            _controllers.Clear();
            _selectedController = null;
        }

    }
}