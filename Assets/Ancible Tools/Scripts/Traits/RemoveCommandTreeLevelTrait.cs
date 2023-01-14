using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Remove Command Tree Level Trait", menuName = "Ancible Tools/Traits/Unit Commands/Remove Command Tree Level")]
    public class RemoveCommandTreeLevelTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _controller.gameObject.SendMessage(RemoveCommandTreeLevelMessage.INSTANCE);
        }
    }
}