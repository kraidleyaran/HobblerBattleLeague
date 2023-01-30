using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "On Status Effect Received Trait", menuName = "Ancible Tools/Traits/Combat/On Event/On Status Effect Received")]
    public class OnStatusEffectReceivedTrait : Trait
    {
        [SerializeField] private StatusEffectType[] _statusEffects = new StatusEffectType[0];
        [SerializeField] private Trait[] _applyOnStatusEffect = new Trait[0];
        [SerializeField] private int _cooldownTicks = 0;
        [SerializeField] private bool _trinket = true;

        private TickTimer _cooldownTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var unique = _statusEffects.Distinct().ToArray();
            foreach (var type in unique)
            {
                SubscribeToType(type);
            }
        }

        public override string GetDescription()
        {
            var description = $"On";
            var unique = _statusEffects.Distinct().ToArray();
            for (var i = 0; i < unique.Length; i++)
            {
                var effect = unique[i];
                if (i < unique.Length - 1)
                {
                    description = unique.Length > 2 ? $"{description} {effect}," : $"{description} {effect}";
                }
                else if (unique.Length > 1)
                {
                    description = $"{description} or {effect} received:";
                }
                else
                {
                    description = $"{description} {effect} received:";
                }
            }
            var traitDescriptions = _applyOnStatusEffect.GetTraitDescriptions();
            for (var i = 0; i < traitDescriptions.Length; i++)
            {
                if (i < traitDescriptions.Length)
                {
                    description = description.Length > 2 ? $"{description} {traitDescriptions[i]}," : $"{description} {traitDescriptions[i]}";
                }
                else if (traitDescriptions.Length > 1)
                {
                    description = $"{description} and {traitDescriptions[i]}";
                }
                else
                {
                    description = $"{description} {traitDescriptions[i]}";
                }
            }

            return description;
        }

        private void ApplyOnEfect()
        {
            if (_cooldownTimer == null)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyOnStatusEffect)
                {
                    addTraitToUnitMsg.Trait = trait;
                    _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);

                if (_cooldownTicks > 0)
                {
                    _cooldownTimer = new TickTimer(_cooldownTicks, 0, null, () =>
                    {
                        _cooldownTimer.Destroy();
                        _cooldownTimer = null;
                    });
                }
            }
        }

        private void SubscribeToType(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<StunMessage>(Stun, _instanceId);
                    break;
                case StatusEffectType.Silence:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<SilenceMessage>(Silence, _instanceId);
                    break;
                case StatusEffectType.Root:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<RootMessage>(Root, _instanceId);
                    break;
                case StatusEffectType.Mute:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<MuteMessage>(Mute, _instanceId);
                    break;
                case StatusEffectType.Disarm:
                    _controller.transform.parent.gameObject.SubscribeWithFilter<DisarmMessage>(Disarm, _instanceId);
                    break;
            }
        }

        private void Stun(StunMessage msg)
        {
            ApplyOnEfect();
        }

        private void Silence(SilenceMessage msg)
        {
            ApplyOnEfect();
        }

        private void Mute(MuteMessage msg)
        {
            ApplyOnEfect();
        }

        private void Disarm(DisarmMessage msg)
        {
            ApplyOnEfect();
        }

        private void Root(RootMessage msg)
        {
            ApplyOnEfect();
        }


    }
}