using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Status Effect Trait", menuName = "Ancible Tools/Traits/Combat/Status Effect")]
    public class StatusEffectTrait : Trait
    {
        [SerializeField] private StatusEffectType _type;
        [SerializeField] private int _ticks = 0;

        private TickTimer _statusEffectTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var canApply = true;
            var statusEffectCheckMsg = MessageFactory.GenerateStatusEffectCheckMsg();
            statusEffectCheckMsg.Type = _type;
            statusEffectCheckMsg.DoAfter = () =>  canApply = true;
            _controller.gameObject.SendMessageTo(statusEffectCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(statusEffectCheckMsg);
            if (canApply)
            {
                SubscribeToMessages();
                var showFloatingTextMsg = MessageFactory.GenerateShowFloatingTextMsg();
                showFloatingTextMsg.Color = ColorFactoryController.GetColorForStatusEffect(_type);
                showFloatingTextMsg.Text = _type.ToFloatingText();
                showFloatingTextMsg.World = _controller.transform.position.ToVector2();
                _controller.gameObject.SendMessage(showFloatingTextMsg);
                MessageFactory.CacheMessage(showFloatingTextMsg);
            }
            else
            {
                RemoveStatusEffect(false);
            }
        }

        private void RemoveStatusEffect(bool finished)
        {
            if (_statusEffectTimer != null)
            {
                _statusEffectTimer.Destroy();
                _statusEffectTimer = null;
            }
            var removeTraitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
            removeTraitByControllerMsg.Controller = _controller;
            _controller.gameObject.SendMessageTo(removeTraitByControllerMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(removeTraitByControllerMsg);
            if (finished)
            {
                var statusEffectFinishedMsg = MessageFactory.GenerateStatusEffectFinishedMsg();
                statusEffectFinishedMsg.Type = _type;
                _controller.gameObject.SendMessageTo(statusEffectFinishedMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(statusEffectFinishedMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<StatusEffectCheckMessage>(StatusEffectCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DispelStatusEffectsMessage>(DispelStatusEffects, _instanceId);
            switch (_type)
            {
                case StatusEffectType.Stun:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanMoveCheckMessage>(CanMoveCheck, _instanceId);
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanCastCheckMessage>(CanCastCheck, _instanceId);
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanAttackCheckMessage>(CanAttack, _instanceId);
                    _controller.gameObject.SendMessageTo(StunMessage.INSTANCE, _controller.transform.parent.gameObject);
                    break;
                case StatusEffectType.Silence:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanCastCheckMessage>(CanCastCheck, _instanceId);
                    _controller.gameObject.SendMessageTo(SilenceMessage.INSTANCE, _controller.transform.parent.gameObject);
                    break;
                case StatusEffectType.Root:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanMoveCheckMessage>(CanMoveCheck, _instanceId);
                    _controller.gameObject.SendMessageTo(RootMessage.INSTANCE, _controller.transform.parent.gameObject);
                    break;
                case StatusEffectType.Mute:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanActivateTrinketCheckMessage>(CanActivateTrinket, _instanceId);
                    _controller.gameObject.SendMessageTo(MuteMessage.INSTANCE, _controller.transform.parent.gameObject);
                    break;
                case StatusEffectType.Disarm:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<CanAttackCheckMessage>(CanAttack, _instanceId);
                    _controller.gameObject.SendMessageTo(DisarmMessage.INSTANCE, _controller.transform.parent.gameObject);
                    break;
            }
            if (_ticks > 0)
            {
                _statusEffectTimer = new TickTimer(_ticks, -1, null, () => {RemoveStatusEffect(true);});
            }
        }

        private void CanMoveCheck(CanMoveCheckMessage msg)
        {
            msg.DoAfter.Invoke();
        }

        private void CanCastCheck(CanCastCheckMessage msg)
        {
            msg.DoAfter.Invoke();
        }

        private void CanActivateTrinket(CanActivateTrinketCheckMessage msg)
        {
            msg.DoAfter.Invoke();
        }

        private void CanAttack(CanAttackCheckMessage msg)
        {
            msg.DoAfter.Invoke();
        }

        private void StatusEffectCheck(StatusEffectCheckMessage msg)
        {
            if (msg.Type == _type)
            {
                msg.DoAfter.Invoke();
            }
        }

        private void DispelStatusEffects(DispelStatusEffectsMessage msg)
        {
            if (msg.Types.Contains(_type))
            {
                RemoveStatusEffect(true);
            }
        }
    }
}