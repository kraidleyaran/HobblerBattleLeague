using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiCraftingWindowController : UiBaseWindow
    {
        public const string FILTER = "UI_CRAFTING_WINDOW";

        public override bool Movable => true;

        [SerializeField] private Text _nameText = null;
        [SerializeField] private UiCraftingRecipeManager _recipeManager;
        [SerializeField] private UiCraftingQueueManager _queueManager;
        [SerializeField] private UiRecipeInfoController _recipeInfoController;

        private GameObject _owner = null;

        public void Setup(GameObject owner)
        {
            _recipeManager.Setup(owner, gameObject);
            _queueManager.Setup(owner);
            _recipeInfoController.Setup(owner, gameObject);

            var queryUnitNameMsg = MessageFactory.GenerateQueryUnitNameMsg();
            queryUnitNameMsg.DoAfter = unitName => _nameText.text = unitName;
            gameObject.SendMessageTo(queryUnitNameMsg, _owner);
            MessageFactory.CacheMessage(queryUnitNameMsg);
        }

        public override void Destroy()
        {
            _queueManager.Destroy();
            _recipeInfoController.Destroy();
            _recipeManager.Destroy();
            base.Destroy();
        }
    }
}