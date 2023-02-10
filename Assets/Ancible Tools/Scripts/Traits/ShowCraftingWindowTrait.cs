using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Crafting Window Trait", menuName = "Ancible Tools/Traits/Ui/Show Crafting Window")]
    public class ShowCraftingWindowTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            var showCraftingWindowMsg = MessageFactory.GenerateShowCraftingWindowMsg();
            showCraftingWindowMsg.Owner = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessage(showCraftingWindowMsg);
            MessageFactory.CacheMessage(showCraftingWindowMsg);
        }
    }
}