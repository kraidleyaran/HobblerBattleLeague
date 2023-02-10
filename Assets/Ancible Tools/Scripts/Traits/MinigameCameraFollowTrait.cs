using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Camera Follow Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Camera Follow")]
    public class MinigameCameraFollowTrait : Trait
    {
        private SetMinigameCameraPositionMessage _setCameraPosMsg = new SetMinigameCameraPositionMessage();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdatePositionMessage>(UpdatePosition, _instanceId);
        }

        private void UpdatePosition(UpdatePositionMessage msg)
        {
            _setCameraPosMsg.Position = msg.Position;
            _controller.gameObject.SendMessage(_setCameraPosMsg);
        }
    }
}