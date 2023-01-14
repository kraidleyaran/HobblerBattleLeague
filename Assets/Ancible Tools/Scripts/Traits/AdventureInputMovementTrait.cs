using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Input Movement Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Input Movement")]
    public class AdventureInputMovementTrait : InputMovementTrait
    {
        private bool _subscribed = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        protected internal override void SubscribeToMessages()
        {
            _controller.gameObject.Subscribe<UpdateWorldStateMessage>(UpdateWorldState);
        }

        private void UpdateWorldState(UpdateWorldStateMessage msg)
        {
            if (msg.State == WorldState.Adventure)
            {
                if (!_subscribed)
                {
                    _subscribed = true;
                    _controller.gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
                }
            }
            else if (_subscribed)
            {
                _subscribed = false;
                _controller.gameObject.Unsubscribe<UpdateInputStateMessage>();
            }
        }
    }
}