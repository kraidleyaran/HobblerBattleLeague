using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Combat Stats Trait", menuName = "Ancible Tools/Traits/Battle/Battle Combat Stats")]
    public class BattleCombatStatsTrait : Trait
    {
        [SerializeField] private CombatStats _startingStats = CombatStats.Zero;

        private CombatStats _baseStats = CombatStats.Zero;
        private CombatStats _bonusStats = CombatStats.Zero;

        private int _currentHealth = 0;
        private int _currentMana = 0;

        private UnitBattleState _battleState = UnitBattleState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _baseStats = _startingStats;
            _currentHealth = _baseStats.Health;
            _currentMana = _baseStats.Mana;
            SubscribeToMessages();
        }

        private void ApplyAdditionalStats(CombatStats stats)
        {
            _currentHealth += stats.Health;
            _currentMana += stats.Mana;
            StatCheck();
        }

        private void StatCheck()
        {
            var allStats = _baseStats + _bonusStats;
            _currentHealth = Mathf.Max(Mathf.Min(allStats.Health, _currentHealth), 1);
            _currentMana = Mathf.Max(Mathf.Min(allStats.Mana, _currentMana), 1);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCombatStatsMessage>(SetCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyCombatStatsMessage>(ApplyCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DamageMessage>(Damage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<HealMessage>(Heal, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHealthMessage>(QueryHealth, _instanceId);
        }

        private void SetCombatStats(SetCombatStatsMessage msg)
        {
            _baseStats = msg.Stats + msg.Accumulated;
            _currentHealth = _baseStats.Health;
            _currentMana = _baseStats.Mana;

            var updateCombatStatsMsg = MessageFactory.GenerateUpdateCombatStatsMsg();
            updateCombatStatsMsg.Base = _baseStats;
            updateCombatStatsMsg.Bonus = _baseStats;
            _controller.gameObject.SendMessageTo(updateCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateCombatStatsMsg);
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
            ApplyAdditionalStats(msg.Stats);

            var updateCombatStatsMsg = MessageFactory.GenerateUpdateCombatStatsMsg();
            updateCombatStatsMsg.Base = _baseStats;
            updateCombatStatsMsg.Bonus = _baseStats;
            _controller.gameObject.SendMessageTo(updateCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateCombatStatsMsg);

            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void Damage(DamageMessage msg)
        {
            if (_currentHealth > 0 && _battleState != UnitBattleState.Dead)
            {
                var resisted = WorldCombatController.CalculateResist(_baseStats + _bonusStats, msg.Amount, msg.Type);
                if (resisted < msg.Amount)
                {
                    var damage = msg.Amount - resisted;
                    _currentHealth -= damage;
                    var reportDamageMsg = MessageFactory.GenerateReportDamageMsg();
                    reportDamageMsg.Amount = damage;
                    reportDamageMsg.Owner = msg.Owner && msg.Owner != _controller.transform.parent.gameObject  ? msg.Owner : null;
                    _controller.gameObject.SendMessageTo(reportDamageMsg, _controller.transform.parent.gameObject);
                    if (msg.Owner)
                    {
                        _controller.gameObject.SendMessageTo(reportDamageMsg, msg.Owner);
                    }
                    MessageFactory.CacheMessage(reportDamageMsg);

                    var showFloatingTextMsg = MessageFactory.GenerateShowFloatingTextMsg();
                    showFloatingTextMsg.Color = ColorFactoryController.NegativeStatColor;
                    showFloatingTextMsg.Text = $"-{damage}";
                    showFloatingTextMsg.World = _controller.transform.parent.position.ToVector2();
                    _controller.gameObject.SendMessage(showFloatingTextMsg);
                    MessageFactory.CacheMessage(showFloatingTextMsg);
                    if (_currentHealth <= 0)
                    {
                        _currentHealth = 0;
                        _controller.gameObject.SendMessageTo(UnitDiedMessage.INSTANCE, _controller.transform.parent.gameObject);

                        var setUnitStateMsg = MessageFactory.GenerateSetUnitBattleStateMsg();
                        setUnitStateMsg.State = UnitBattleState.Dead;
                        _controller.gameObject.SendMessageTo(setUnitStateMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(setUnitStateMsg);
                    }
                    else
                    {
                        _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
                    }
                }
            }
            
        }

        private void Heal(HealMessage msg)
        {
            if (_battleState != UnitBattleState.Dead)
            {
                var reportHealMsg = MessageFactory.GenerateReportHealMsg();
                reportHealMsg.Amount = msg.Amount;
                reportHealMsg.Owner = msg.Owner && msg.Owner != _controller.transform.parent.gameObject ? msg.Owner : null;
                _controller.gameObject.SendMessageTo(reportHealMsg, _controller.transform.parent.gameObject);
                if (msg.Owner)
                {
                    _controller.gameObject.SendMessageTo(reportHealMsg, msg.Owner);
                }
                MessageFactory.CacheMessage(reportHealMsg);
                var showFloatingTextMsg = MessageFactory.GenerateShowFloatingTextMsg();
                showFloatingTextMsg.Color = ColorFactoryController.BonusStat;
                showFloatingTextMsg.Text = $"+{msg.Amount}";
                showFloatingTextMsg.World = _controller.transform.position.ToVector2();
                _controller.gameObject.SendMessage(showFloatingTextMsg);
                MessageFactory.CacheMessage(showFloatingTextMsg);
                _currentHealth = Mathf.Max(Mathf.Min(_currentHealth + msg.Amount, _baseStats.Health + _bonusStats.Health), 0);
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            _battleState = msg.State;
        }

        private void QueryHealth(QueryHealthMessage msg)
        {
            msg.DoAfter.Invoke(_currentHealth, (_baseStats + _bonusStats).Health);
        }

    }
}