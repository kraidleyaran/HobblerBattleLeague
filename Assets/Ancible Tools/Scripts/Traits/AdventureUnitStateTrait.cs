using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Unit State Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Unit State")]
    public class AdventureUnitStateTrait : Trait
    {
        [SerializeField] private AdventureUnitType _unitType = AdventureUnitType.NPC;

        private AdventureUnitState _unitState = AdventureUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAdventureUnitStateMessage>(SetAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAdventureUnitStateMessage>(QueryAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAdventureUnitTypeMessage>(QueryAdventureUnitType, _instanceId);
        }

        private void SetAdventureUnitState(SetAdventureUnitStateMessage msg)
        {
            if (_unitState != msg.State)
            {
                _unitState = msg.State;
                var updateAdventureUnitStateMsg = MessageFactory.GenerateUpdateAdventureUnitStateMsg();
                updateAdventureUnitStateMsg.State = _unitState;
                _controller.gameObject.SendMessageTo(updateAdventureUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateAdventureUnitStateMsg);
            }
        }

        private void QueryAdventureUnitState(QueryAdventureUnitStateMessage msg)
        {
            msg.DoAfter.Invoke(_unitState);
        }

        private void QueryAdventureUnitType(QueryAdventureUnitTypeMessage msg)
        {
            msg.DoAfter.Invoke(_unitType);
        }
    }
}