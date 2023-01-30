﻿using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "On Healing Received Trait", menuName = "Ancible Tools/Traits/Combat/On Event/On Healing Received")]
    public class OnHealingReceivedTrait : Trait
    {
        [SerializeField] private int _amount = 0;
        [SerializeField] private Trait[] _applyOnAmount;
        [SerializeField] private int _cooldownTicks = 0;
        [SerializeField] private bool _trinket = true;

        private int _amountDone = 0;
        private TickTimer _cooldownTimer = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        public override string GetDescription()
        {
            var description = $"On {_amount} healing received:{Environment.NewLine}";
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

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportHealMessage>(ReportHeal, _instanceId);
        }

        private void ReportHeal(ReportHealMessage msg)
        {
            if (!msg.Owner || msg.Owner != _controller.transform.parent.gameObject && _cooldownTimer == null)
            {
                _amountDone += msg.Amount;
                if (_amountDone >= _amount)
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
                        _amountDone -= _amount;
                        var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                        foreach (var trait in _applyOnAmount)
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
            }
        }
    }
}