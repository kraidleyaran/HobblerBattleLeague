using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Miningame Casting Trait", menuName = "Ancible Tools/Traits/Minigame/Combat/Minigame Casting")]
    public class MinigameCastingTrait : Trait
    {
        private TickTimer _castingTimer = null;

        private MinigameUnitState _unitState = MinigameUnitState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void OnCastFinish(AbilityInstance ability, GameObject target)
        {
            ability.UseAbility(_controller.transform.parent.gameObject, target);
            _castingTimer.Destroy();
            _castingTimer = null;

            var applyManaMsg = MessageFactory.GenerateApplyManaMsg();
            applyManaMsg.Amount = ability.Instance.ManaCost * -1;
            _controller.gameObject.SendMessageTo(applyManaMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyManaMsg);

            _controller.gameObject.SendMessageTo(ActivateGlobalCooldownMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void InterruptCast()
        {
            if (_castingTimer != null)
            {
                _castingTimer.Destroy();
                _castingTimer = null;
                _controller.gameObject.SendMessageTo(CastInterruptedMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

            if (_unitState == MinigameUnitState.Casting)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitStateMsg.State = MinigameUnitState.Idle;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<CastAbilityMessage>(CastAbility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMinigameUnitStateMessage>(UpdateMinigameUnitState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SilenceMessage>(Silence, _instanceId);
        }

        private void CastAbility(CastAbilityMessage msg)
        {
            if (_unitState == MinigameUnitState.Idle)
            {
                if (_castingTimer != null)
                {
                    _castingTimer.Destroy();
                    _castingTimer = null;
                }

                var ability = msg.Ability;
                var target = msg.Target;
                _castingTimer = new TickTimer(msg.Ability.Instance.CastTime, 0, () => OnCastFinish(ability, target), null, false);
                var updateUnitCastTimerMsg = MessageFactory.GenerateUpdateUnitCastTimerMsg();
                updateUnitCastTimerMsg.CastTimer = _castingTimer;
                updateUnitCastTimerMsg.Icon = ability.Instance.Icon;
                updateUnitCastTimerMsg.Name = ability.Instance.DisplayName;
                _controller.gameObject.SendMessageTo(updateUnitCastTimerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(updateUnitCastTimerMsg);

                var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setMinigameUnitStateMsg.State = MinigameUnitState.Casting;
                _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setMinigameUnitStateMsg);
            }

        }

        private void UpdateMinigameUnitState(UpdateMinigameUnitStateMessage msg)
        {
            if (msg.State == MinigameUnitState.Disabled)
            {
                if (_castingTimer != null)
                {
                    _castingTimer.Destroy();
                    _castingTimer = null;
                }
            }
            _unitState = msg.State;
            
        }

        private void Silence(SilenceMessage msg)
        {
            InterruptCast();
        }

        private void Stun(StunMessage msg)
        {
            InterruptCast();
        }
    }
}