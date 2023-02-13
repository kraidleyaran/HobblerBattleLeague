using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Ai State Trait", menuName = "Ancible Tools/Traits/Adventure/Ai/Adventure Ai State")]
    public class AdventureAiStateTrait : Trait
    {
        private AdventureAiState _aiState = AdventureAiState.Wander;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAdventureAiStateMessage>(SetAdventureAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAdventureAiStateMessage>(QueryAdventureAiState, _instanceId);
        }

        private void SetAdventureAiState(SetAdventureAiStateMessage msg)
        {
            if (_aiState != msg.State)
            {
                _aiState = msg.State;
                var updateAdventureAiStateMsg = MessageFactory.GenerateUpdateAdventureAiStateMsg();
                updateAdventureAiStateMsg.State = _aiState;
                _controller.gameObject.SendMessageTo(updateAdventureAiStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateAdventureAiStateMsg);
            }
            
        }

        private void QueryAdventureAiState(QueryAdventureAiStateMessage msg)
        {
            msg.DoAfter.Invoke(_aiState);
        }
    }
}