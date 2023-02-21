using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Adventure Global Cooldown Trait", menuName = "Ancible Tools/Traits/Adventure/Adventure Global Cooldown")]
    public class AdventureGlobalCooldownTrait : Trait
    {
        private TickTimer _globalCooldown = null;
        private AdventureUnitState _unitState = AdventureUnitState.Idle;
        [SerializeField] private int _ticks = 1;

        private int _bonus = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _globalCooldown = new TickTimer(_ticks, 0, FinishGlobalCooldown, null, false);
            SubscribeToMessages();
        }

        private void FinishGlobalCooldown()
        {
            if (_unitState == AdventureUnitState.GlobalCooldown)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ActivateGlobalCooldownMessage>(ActivateGlobalCooldown, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateAdventureUnitStateMessage>(UpdateAdventureUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyGlobalCooldownBonusMessage>(ApplyGlobalCooldownBonus, _instanceId);
        }

        private void ActivateGlobalCooldown(ActivateGlobalCooldownMessage msg)
        {
            _globalCooldown.Play();
            if (_unitState != AdventureUnitState.GlobalCooldown && _unitState != AdventureUnitState.Disabled)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetAdventureUnitStateMsg();
                setUnitStateMsg.State = AdventureUnitState.GlobalCooldown;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
            
        }

        private void UpdateAdventureUnitState(UpdateAdventureUnitStateMessage msg)
        {
            _unitState = msg.State;
        }

        private void ApplyGlobalCooldownBonus(ApplyGlobalCooldownBonusMessage msg)
        {
            if (msg.Permanent)
            {
                _ticks += msg.Bonus;
            }
            else
            {
                _bonus += msg.Bonus;
            }

            _globalCooldown.SetTicksPerCycle(_ticks + _bonus);
        }

        public override void Destroy()
        {
            _globalCooldown?.Destroy();
            _globalCooldown = null;
            base.Destroy();
        }
    }
}