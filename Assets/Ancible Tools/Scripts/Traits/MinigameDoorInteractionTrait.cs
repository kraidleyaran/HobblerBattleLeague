using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Door Interaction Trait", menuName = "Ancible Tools/Traits/Minigame/Interaction/Minigame Door Interaction")]
    public class MinigameDoorInteractionTrait : MinigameInteractionTrait
    {
        private MinigameUnitState _unitState = MinigameUnitState.Idle;

        protected internal override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
        }

        protected internal override void MinigameInteract(MinigameInteractMessage msg)
        {
            if (_unitState != MinigameUnitState.Interact && _unitState != MinigameUnitState.Disabled)
            {
                var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setMinigameUnitStateMsg.State = MinigameUnitState.Interact;
                _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setMinigameUnitStateMsg);

                var owner = msg.Owner;
                var doBumpMsg = MessageFactory.GenerateDoBumpMsg();
                doBumpMsg.OnBump = OpenDoor;
                doBumpMsg.DoAfter = () => { CleanUp(owner); };
                doBumpMsg.Direction = msg.Direction;
                _controller.gameObject.SendMessageTo(doBumpMsg, owner);
                MessageFactory.CacheMessage(doBumpMsg);
                base.MinigameInteract(msg);
            }
        }

        private void OpenDoor()
        {
            var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
            setMinigameUnitStateMsg.State = MinigameUnitState.Disabled;
            _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setMinigameUnitStateMsg);
        }

        private void CleanUp(GameObject owner)
        {
            var setOwnerUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
            setOwnerUnitStateMsg.State = MinigameUnitState.Idle;
            _controller.gameObject.SendMessageTo(setOwnerUnitStateMsg, owner);
            MessageFactory.CacheMessage(setOwnerUnitStateMsg);
        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            _unitState = msg.State;
        }
    }
}