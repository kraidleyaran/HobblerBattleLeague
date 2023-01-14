using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Unit State Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Unit State")]
    public class MinigameUnitStateTrait : Trait
    {
        private MinigameUnitState _state = MinigameUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMinigameUnitStateMessage>(SetMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMinigameUnitStateMessage>(QueryMinigameUnitState, _instanceId);
        }

        private void SetMinigameUnitState(SetMinigameUnitStateMessage msg)
        {
            if (_state != msg.State)
            {
                var prevState = _state;
                _state = msg.State;
                var updateMinigameUnitStateMsg = MessageFactory.GenerateUpdateMinigameUnitStateMsg();
                updateMinigameUnitStateMsg.State = _state;
                _controller.gameObject.SendMessageTo(updateMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateMinigameUnitStateMsg);

                if (_state == MinigameUnitState.Disabled)
                {
                    _controller.transform.parent.gameObject.SetActive(false);
                }
                else if (prevState == MinigameUnitState.Disabled)
                {
                    _controller.transform.parent.gameObject.SetActive(true);
                }
            }
        }

        private void QueryMinigameUnitState(QueryMinigameUnitStateMessage msg)
        {
            msg.DoAfter.Invoke(_state);
        }
    }
}