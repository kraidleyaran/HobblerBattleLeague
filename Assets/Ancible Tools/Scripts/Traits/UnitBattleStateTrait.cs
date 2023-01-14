using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Unit Battle State Trait", menuName = "Ancible Tools/Traits/Battle/Unit Battle State")]
    public class UnitBattleStateTrait : Trait
    {
        private UnitBattleState _state = UnitBattleState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitBattleStateMessage>(SetUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryUnitBattleStateMessage>(QueryUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
        }

        private void SetUnitBattleState(SetUnitBattleStateMessage msg)
        {
            if (_state != msg.State)
            {
                _state = msg.State;
                var updateUnitBattleStateMsg = MessageFactory.GenerateUpdateUnitBattleStateMsg();
                updateUnitBattleStateMsg.State = _state;
                _controller.gameObject.SendMessageTo(updateUnitBattleStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateUnitBattleStateMsg);
            }
        }

        private void QueryUnitBattleState(QueryUnitBattleStateMessage msg)
        {
            msg.DoAfter.Invoke(_state);
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            _controller.transform.parent.gameObject.SetActive(false);
        }
    }
}