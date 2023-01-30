using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Global Cooldown Trait", menuName = "Ancible Tools/Traits/Battle/Battle Battle Global Cooldown")]
    public class BattleGlobalCooldownTrait : Trait
    {
        private TickTimer _globalCooldown = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _globalCooldown = new TickTimer(BattleLeagueController.GlobalCooldownTicks, 0, CooldownFinished, null,false, false);
            SubscribeToMessages();
        }

        private void CooldownFinished()
        {
            var setUnitBattleStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
            setUnitBattleStateMsg.State = UnitBattleState.Active;
            _controller.gameObject.SendMessageTo(setUnitBattleStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitBattleStateMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ActivateGlobalCooldownMessage>(ActivateGlobalCooldown, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryGlobalCooldownMessage>(QueryGlobalCooldown, _instanceId);
        }

        private void ActivateGlobalCooldown(ActivateGlobalCooldownMessage msg)
        {
            var setUnitBattleStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
            setUnitBattleStateMsg.State = UnitBattleState.Global;
            _controller.gameObject.SendMessageTo(setUnitBattleStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setUnitBattleStateMsg);
            _globalCooldown.Play();
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            if (msg.State == UnitBattleState.Dead)
            {
                _globalCooldown?.Destroy();
                _globalCooldown = null;
            }
        }

        private void QueryGlobalCooldown(QueryGlobalCooldownMessage msg)
        {
            msg.DoAfter.Invoke(_globalCooldown);
        }

        public override void Destroy()
        {
            if (_globalCooldown != null)
            {
                _globalCooldown.Destroy();
                _globalCooldown = null;
            }
            base.Destroy();
        }
    }
}