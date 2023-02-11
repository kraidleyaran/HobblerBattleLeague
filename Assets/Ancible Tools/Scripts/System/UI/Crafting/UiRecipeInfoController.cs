using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiRecipeInfoController : MonoBehaviour
    {
        private const string FILTER = "UI_RECIPE_INFO_CONTROLLER";

        [SerializeField] private Text _recipeNameText;
        [SerializeField] private UiStashItemController _recipeItemController;
        [SerializeField] private UiStashItemController _stashItemTemplate;
        [SerializeField] private GridLayoutGroup _materialsGrid;
        [SerializeField] private RectTransform _materialsContent;
        [SerializeField] private Text _goldCostText;
        [SerializeField] private Text _tickCostText;
        [SerializeField] private InputField _quantityInputText;
        [SerializeField] private Button _craftButton;
        [SerializeField] private GameObject _objectGrouping;
        [SerializeField] private int _maxQuantity = 999;

        private GameObject _owner;
        private GameObject _parent;
        private string _filter = string.Empty;

        private CraftingRecipe _selectedRecipe = null;
        private int _quantity = 0;
        private UiStashItemController[] _materialControllers = new UiStashItemController[0];

        public void Setup(GameObject owner, GameObject parent)
        {
            _owner = owner;
            _parent = parent;
            _craftButton.interactable = false;
            _filter = $"{FILTER}-{_owner.GetInstanceID()}";
            _objectGrouping.gameObject.SetActive(false);
            SubscribeToMessages();
        }

        public void Craft()
        {
            if (_selectedRecipe != null)
            {
                if(IsCraftable())
                {
                    var queueCraftMsg = MessageFactory.GenerateQueueCraftingRecipeMsg();
                    queueCraftMsg.Recipe = _selectedRecipe;
                    queueCraftMsg.Stack = _quantity;
                    gameObject.SendMessageTo(queueCraftMsg, _owner);
                    MessageFactory.CacheMessage(queueCraftMsg);
                }

            }
        }

        private void UpdateSelectedRecipe()
        {
            _recipeItemController.Setup(_selectedRecipe.Item, false);
            _recipeNameText.text = $"{_selectedRecipe.Item.Item.DisplayName}";
            _quantityInputText.SetTextWithoutNotify($"{1}");
            UpdateMaterials();
            UpdateQuantity();
            _objectGrouping.gameObject.SetActive(true);
        }

        private void UpdateMaterials()
        {
            foreach (var controller in _materialControllers)
            {
                Destroy(controller.gameObject);
            }

            if (_selectedRecipe)
            {
                var controllers = new List<UiStashItemController>();
                foreach (var material in _selectedRecipe.RequiredItems)
                {
                    var controller = Instantiate(_stashItemTemplate, _materialsGrid.transform);
                    controller.Setup(material, false);
                    controllers.Add(controller);
                }

                _materialControllers = controllers.ToArray();
            }
        }

        public void UpdateQuantity()
        {
            _quantity = int.TryParse(_quantityInputText.text, out var quantity) ? Mathf.Min(Mathf.Max(quantity, 0), quantity, _maxQuantity) : 0;

            var goldCost = _selectedRecipe.Cost * _quantity;
            _goldCostText.text = $"{goldCost:N0}";

            var timeCost = _selectedRecipe.CraftingTicks * _quantity;
            _tickCostText.text = $"{timeCost:N0}";
            UpdateCraftButton();
        }

        private void UpdateCraftButton()
        {
            _craftButton.interactable = IsCraftable();
        }

        private bool IsCraftable()
        {
            if (_selectedRecipe != null)
            {
                var craftable = _quantity > 0;
                if (craftable)
                {
                    var goldCost = _selectedRecipe.Cost * _quantity;
                    craftable = goldCost <= WorldStashController.Gold;
                }
                if (craftable)
                {
                    foreach (var item in _selectedRecipe.RequiredItems)
                    {
                        craftable = WorldStashController.HasItem(item.Item, item.Stack * _quantity);
                        if (!craftable)
                        {
                            break;
                        }
                    }
                }

                return craftable;
            }
            return false;
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<GoldUpdatedMessage>(GoldUpdated);
            gameObject.Subscribe<StashUpdatedMessage>(WorldStashUpdated);
            _parent.SubscribeWithFilter<SetSelectedCraftingRecipeControllerMessage>(SetSelectedCraftingRecipeController, _filter);

        }

        private void SetSelectedCraftingRecipeController(SetSelectedCraftingRecipeControllerMessage msg)
        {
            _selectedRecipe = msg.Controller.Recipe;
            UpdateSelectedRecipe();
        }

        private void GoldUpdated(GoldUpdatedMessage msg)
        {
            _craftButton.interactable = IsCraftable();
        }

        private void WorldStashUpdated(StashUpdatedMessage msg)
        {
            _craftButton.interactable = IsCraftable();
        }

        public void Destroy()
        {
            gameObject.UnsubscribeFromAllMessages();
            _parent.UnsubscribeFromAllMessagesWithFilter(_filter);
            _filter = null;
            _owner = null;
            _selectedRecipe = null;
        }

    }
}