using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    public class MinigameInteractionTrait : Trait
    {
        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        protected internal virtual void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<MinigameInteractMessage>(MinigameInteract, _instanceId);
        }

        protected internal virtual void MinigameInteract(MinigameInteractMessage msg)
        {
            var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
            setMinigameUnitStateMsg.State = MinigameUnitState.Interact;
            _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, msg.Owner);
            MessageFactory.CacheMessage(setMinigameUnitStateMsg);
        }
    }
}