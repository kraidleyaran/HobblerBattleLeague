using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "On Damage Received Trait", menuName = "Ancible Tools/Traits/Combat/On Event/On Damage Received")]
    public class OnDamageReceivedTrait : Trait
    {
        [SerializeField] private int _requiredAmount = 0;
        [SerializeField] private Trait[] _applyOnAmount;
        [SerializeField] private int _cooldownTicks = 0;
        [SerializeField] private bool _trinket = true;

        private int _damageTaken = 0;
        private TickTimer _cooldownTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }


        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportDamageMessage>(ReportDamage, _instanceId);
        }

        private void ReportDamage(ReportDamageMessage msg)
        {
            if (_cooldownTimer == null && (!msg.Owner || msg.Owner != _controller.transform.parent.gameObject))
            {

                _damageTaken += msg.Amount;
                if (_damageTaken >= _requiredAmount)
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
                        _damageTaken = 0;
                        var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                        foreach (var trait in _applyOnAmount)
                        {
                            addTraitToUnitMsg.Trait = trait;
                            _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                        }
                        MessageFactory.CacheMessage(addTraitToUnitMsg);
                        if (_cooldownTicks > 0)
                        {
                            _cooldownTimer = new TickTimer(_cooldownTicks, 0, () =>
                            {
                                _cooldownTimer.Destroy();
                                _cooldownTimer = null;
                            }, null, false);
                        }
                    }

                }

                
            }
        }

        public override string GetDescription()
        {
            var description = $"On {_requiredAmount} damage received:{Environment.NewLine}";
            var traitDescriptions = _applyOnAmount.GetTraitDescriptions();
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
            if (_cooldownTicks > 0)
            {
                description = $"{description}{Environment.NewLine}Cooldown: {TickController.TickRate * _cooldownTicks:N}s";
            }
            return description;
        }
    }
}