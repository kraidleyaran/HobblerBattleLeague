using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Monster State Trait", menuName = "Ancible Tools/Traits/Hobbler/State/Monster State")]
    public class MonsterStateTrait : Trait
    {
        private MonsterState _state = MonsterState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMonsterStateMessage>(SetMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMonsterStateMessage>(QueryMonsterState, _instanceId);
        }

        private void SetMonsterState(SetMonsterStateMessage msg)
        {
            if (_state != msg.State)
            {
                _state = msg.State;
                var updateMonsterStateMsg = MessageFactory.GenerateUpdateMonsterStateMsg();
                updateMonsterStateMsg.State = _state;
                _controller.gameObject.SendMessageTo(updateMonsterStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateMonsterStateMsg);
            }
        }

        private void QueryMonsterState(QueryMonsterStateMessage msg)
        {
            msg.DoAfter.Invoke(_state);
        }
    }
}