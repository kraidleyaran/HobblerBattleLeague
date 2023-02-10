using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Crafting
{
    public class UiCraftingWindowController : UiBaseWindow
    {
        public override bool Movable => true;

        [SerializeField] private UiCraftingRecipeManager _recipeManager;
        [SerializeField] private UiCraftingQueueManager _queueManager;
        [SerializeField] private UiRecipeInfoController _recipeInfoController;

        private GameObject _owner = null;

        public void Setup(GameObject owner)
        {
            _recipeManager.Setup(owner, gameObject);
            _queueManager.Setup(owner);
            _recipeInfoController.Setup(owner, gameObject);
        }

        public override void Destroy()
        {
            _queueManager.Destroy();
            _recipeInfoController.Destroy();
            _recipeInfoController.Destroy();
            base.Destroy();
        }
    }
}