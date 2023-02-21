using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "On Percent Mana Trait", menuName = "Ancible Tools/Traits/Combat/On Event/On Percent Mana")]
    public class OnPercentManaTrait : Trait
    {
        [SerializeField] [Range(0f, 1f)] private float _percent = .5f;
        [SerializeField] private ComparisonType _comparison = ComparisonType.LessThanOrEqual;
        [SerializeField] private Trait[] _applyOnPercent = new Trait[0];
        [SerializeField] private bool _trinket = false;
        [SerializeField] private int _cooldownTicks = 100;

        private TickTimer _cooldown = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        public override string GetDescription()
        {
            var description = $"On {_percent:F} % Mana: ";
            var traitDescription = string.Empty;
            var traits = _applyOnPercent.GetTraitDescriptions();
            for (var i = 0; i < traits.Length; i++)
            {
                var trait = traits[i];
                if (string.IsNullOrEmpty(traitDescription))
                {
                    traitDescription = traits.Length > 1 ? trait : $"{trait}";
                }
                else if (i <= traits.Length - 1)
                {
                    traitDescription = $"{traitDescription}, {trait}";
                }
                else
                {
                    traitDescription = $"{traitDescription}, and {trait}";
                }
            }

            description = $"{description}{traitDescription}";
            if (_cooldownTicks > 0)
            {
                description = $"{description}{Environment.NewLine}Cooldown: {TickController.TickRate * _cooldownTicks:N}s";
            }
            return description;
        }

        private void CooldownFinished()
        {
            _cooldown?.Destroy();
            _cooldown = null;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateManaMessage>(UpdateMana, _instanceId);
        }

        private void UpdateMana(UpdateManaMessage msg)
        {
            var percent = (float)msg.Current / msg.Max;
            if (_cooldown == null && percent.EqualityCompare(_comparison, _percent))
            {
                var canActivate = true;
                if (_trinket)
                {
                    var canActivateTrinketMsg = MessageFactory.GenerateCanActivateTrinketCheckMsg();
                    canActivateTrinketMsg.DoAfter = () => { canActivate = false; };
                    _controller.gameObject.SendMessageTo(canActivateTrinketMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(canActivateTrinketMsg);
                }

                if (canActivate)
                {
                    var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                    foreach (var trait in _applyOnPercent)
                    {
                        addTraitToUnitMsg.Trait = trait;
                        _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                    }
                    MessageFactory.CacheMessage(addTraitToUnitMsg);

                    if (_cooldownTicks > 0)
                    {
                        _cooldown = new TickTimer(_cooldownTicks, 0, CooldownFinished, null);
                    }
                }
            }
        }

        public override void Destroy()
        {
            _cooldown?.Destroy();
            _cooldown = null;
            base.Destroy();
        }
    }
}