using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Global Cooldown Trait", menuName = "Ancible Tools/Traits/Minigame/Minigame Global Cooldown")]
    public class MinigameGlobalCooldownTrait : Trait
    {
        [SerializeField] private int _globalCooldown = 0;

        private TickTimer _cooldownTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _cooldownTimer = new TickTimer(_globalCooldown, 0, CooldownFinished, null, false);
            SubscribeToMessages();
        }

        private void CooldownFinished()
        {
            var setMinigameUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
            setMinigameUnitStateMsg.State = MinigameUnitState.Idle;
            _controller.gameObject.SendMessageTo(setMinigameUnitStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setMinigameUnitStateMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ActivateGlobalCooldownMessage>(ActivateGlobalCooldown, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryGlobalCooldownMessage>(QueryGlobalCooldown, _instanceId);
        }

        private void ActivateGlobalCooldown(ActivateGlobalCooldownMessage msg)
        {
            if (_cooldownTimer != null)
            {
                var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                setUnitStateMsg.State = MinigameUnitState.GlobalCooldown;
                _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setUnitStateMsg);
                _cooldownTimer.Play();
            }
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            _cooldownTimer.Destroy();
            _cooldownTimer = null;
        }

        private void QueryGlobalCooldown(QueryGlobalCooldownMessage msg)
        {
            msg.DoAfter.Invoke(_cooldownTimer);
        }



        public override void Destroy()
        {
            if (_cooldownTimer != null)
            {
                _cooldownTimer.Destroy();
                _cooldownTimer = null;
            }
            base.Destroy();
        }
    }
}