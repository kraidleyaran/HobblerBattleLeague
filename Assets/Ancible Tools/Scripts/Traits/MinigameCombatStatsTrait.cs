using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Miningame Combat Stats Trait", menuName = "Ancible Tools/Traits/Minigame/Combat/Minigame Combat Stats")]
    public class MinigameCombatStatsTrait : Trait
    {
        [SerializeField] private CombatStats _startingStats = CombatStats.Zero;
        [SerializeField] private CombatAlignment _alignment = CombatAlignment.Player;
        [SerializeField] private bool _disableOnDeath = false;

        private CombatStats _baseStats = CombatStats.Zero;
        private CombatStats _bonusStats = CombatStats.Zero;

        private int _currentHealth = 0;
        private int _currentMana = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _baseStats = _startingStats;
            _currentHealth = _baseStats.Health;
            _currentMana = _baseStats.Mana;
            SubscribeToMessages();
        }

        private void UpdateStats(bool refresh)
        {
            var updateCombatStatsMsg = MessageFactory.GenerateUpdateCombatStatsMsg();
            updateCombatStatsMsg.Base = _baseStats;
            updateCombatStatsMsg.Bonus = _bonusStats;
            updateCombatStatsMsg.Genetics = GeneticCombatStats.Zero;
            _controller.gameObject.SendMessageTo(updateCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateCombatStatsMsg);
            if (refresh)
            {
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyCombatStatsMessage>(ApplyCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCombatStatsMessage>(QueryCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCombatStatsMessage>(SetCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DamageMessage>(Damage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBonusDamageMessage>(QueryBonusDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCombatAlignmentMessage>(QueryCombatAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCombatAlignmentMessage>(SetCombatAlignment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryManaMessage>(QueryMana, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHealthMessage>(QueryHealth, _instanceId);
        }

        private void ApplyCombatStats(ApplyCombatStatsMessage msg)
        {
            if (msg.Bonus)
            {
                _bonusStats += msg.Stats;
            }
            else
            {
                _baseStats += msg.Stats;
            }
            UpdateStats(true);
        }

        private void QueryCombatStats(QueryCombatStatsMessage msg)
        {
            msg.DoAfter.Invoke(_baseStats, _baseStats, GeneticCombatStats.Zero);
        }

        private void SetCombatStats(SetCombatStatsMessage msg)
        {
            _baseStats = msg.Stats;
            UpdateStats(false);
        }

        private void Damage(DamageMessage msg)
        {
            if (_currentHealth > 0)
            {
                var amount = WorldCombatController.CalculateResist(_baseStats + _bonusStats, msg.Amount, msg.Type);
                if (amount < msg.Amount)
                {
                    var damage = msg.Amount - amount;

                    var reportDamageMsg = MessageFactory.GenerateReportDamageMsg();
                    reportDamageMsg.Amount = damage;
                    reportDamageMsg.Owner = msg.Owner;
                    _controller.gameObject.SendMessageTo(reportDamageMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(reportDamageMsg);

                    _currentHealth -= damage;
                    _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);

                    if (_currentHealth <= 0)
                    {
                        _controller.gameObject.SendMessageTo(UnitDiedMessage.INSTANCE, _controller.transform.parent.gameObject);
                        if (_disableOnDeath)
                        {
                            var setUnitStateMsg = MessageFactory.GenerateSetMinigameUnitStateMsg();
                            setUnitStateMsg.State = MinigameUnitState.Disabled;
                            _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setUnitStateMsg);
                        }
                        //TODO: Destroy depending on the unit type, probably need a message
                        //TODO: Need to show the damage type too - just report the damage in case it's needed for other purposes
                        Debug.Log("Unit has died");
                    }
                }
            }

        }

        private void QueryBonusDamage(QueryBonusDamageMessage msg)
        {
            msg.DoAfter.Invoke(WorldCombatController.CalculateDamage(_baseStats + _bonusStats, msg.Type));
        }

        private void QueryCombatAlignment(QueryCombatAlignmentMessage msg)
        {
            msg.DoAfter.Invoke(_alignment);
        }

        private void SetCombatAlignment(SetCombatAlignmentMessage msg)
        {
            _alignment = msg.Alignment;
            var updateAlignmentMsg = MessageFactory.GenerateUpdateCombatAlignmentMsg();
            updateAlignmentMsg.Alignment = _alignment;
            _controller.gameObject.SendMessageTo(updateAlignmentMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateAlignmentMsg);
        }

        private void QueryHealth(QueryHealthMessage msg)
        {
            msg.DoAfter.Invoke(_currentHealth, _baseStats.Health + _bonusStats.Health);
        }

        private void QueryMana(QueryManaMessage msg)
        {
            msg.DoAfter.Invoke(_currentMana, _baseStats.Mana + _bonusStats.Mana);
        }

    }
}