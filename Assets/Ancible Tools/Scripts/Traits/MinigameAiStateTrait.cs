using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.AI;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Ai State Trait", menuName = "Ancible Tools/Traits/Minigame/Ai/Minigame Ai State")]
    public class MinigameAiStateTrait : Trait
    {
        private MinigameAiState _aiState = MinigameAiState.Wander;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMinigameAiStateMessage>(SetMinigameAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMinigameAiStateMessage>(QueryMinigameAiState, _instanceId);
        }

        private void SetMinigameAiState(SetMinigameAiStateMessage msg)
        {
            if (_aiState != msg.State)
            {
                _aiState = msg.State;
                var updateMinigameAiStateMsg = MessageFactory.GenerateUpdateMinigameAiStateMsg();
                updateMinigameAiStateMsg.State = _aiState;
                _controller.gameObject.SendMessageTo(updateMinigameAiStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateMinigameAiStateMsg);
            }
        }

        private void QueryMinigameAiState(QueryMinigameAiStateMessage msg)
        {
            msg.DoAfter.Invoke(_aiState);
        }
    }
}