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
        private float _currentMana = 0;

        private UnitBattleState _battleState = UnitBattleState.Active;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _baseStats = _startingStats;
            _currentHealth = _baseStats.Health;
            _currentMana = 0;
            SubscribeToMessages();
        }

        private void ApplyAdditionalStats(CombatStats stats)
        {
            _currentHealth += stats.Health;
            //_currentMana += stats.Mana;
            StatCheck();
        }

        private void StatCheck()
        {
            var allStats = _baseStats + _bonusStats;
            _currentHealth = Mathf.Max(Mathf.Min(allStats.Health, _currentHealth), 1);
            _currentMana = Mathf.Max(Mathf.Min(allStats.Mana, _currentMana), 0);
        }

        private void UpdateHealth()
        {
            var updateHealthMsg = MessageFactory.GenerateUpdateHealthMsg();
            updateHealthMsg.Current = _currentHealth;
            updateHealthMsg.Max = _baseStats.Health + _bonusStats.Health;
            _controller.gameObject.SendMessageTo(updateHealthMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateHealthMsg);
        }

        private void UpdateMana()
        {
            var updateManaMsg = MessageFactory.GenerateUpdateManaMsg();
            updateManaMsg.Current = (int)_currentMana;
            updateManaMsg.Max = _baseStats.Mana + _bonusStats.Mana;
            _controller.gameObject.SendMessageTo(updateManaMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateManaMsg);
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetCombatStatsMessage>(SetCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyCombatStatsMessage>(ApplyCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<DamageMessage>(Damage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<HealMessage>(Heal, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateUnitBattleStateMessage>(UpdateUnitBattleState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHealthMessage>(QueryHealth, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ReportDamageMessage>(ReportDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryManaMessage>(QueryMana, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyManaMessage>(ApplyMana, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCombatStatsMessage>(QueryCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBonusDamageMessage>(QueryBonusDamage, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBonusHealMessage>(QueryBonusHeal, _instanceId);
        }

        private void SetCombatStats(SetCombatStatsMessage msg)
        {
            _baseStats = msg.Stats + msg.Accumulated;
            _currentHealth = _baseStats.Health;
            _currentMana = 0;

            var updateCombatStatsMsg = MessageFactory.GenerateUpdateCombatStatsMsg();
            updateCombatStatsMsg.Base = _baseStats;
            updateCombatStatsMsg.Bonus = _bonusStats;
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
            updateCombatStatsMsg.Bonus = _bonusStats;
            _controller.gameObject.SendMessageTo(updateCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateCombatStatsMsg);

            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void Damage(DamageMessage msg)
        {
            if (_currentHealth > 0 && _battleState != UnitBattleState.Dead)
            {
                var mana = BattleLeagueManager.GetManaFromDamageTaken(msg.Amount);
                _currentMana = Mathf.Min(mana + _currentMana, _baseStats.Mana + _bonusStats.Mana);
                UpdateMana();
                var resisted = WorldCombatController.CalculateResist(_baseStats + _bonusStats, msg.Amount, msg.Type);
                if (resisted < msg.Amount)
                {
                    var damage = msg.Amount - resisted;
                    var instanceDamage = new WorldInstance<int> {Instance = damage};
                    var absorbDamageCheckMsg = MessageFactory.GenerateAbsorbedDamageCheckMsg();
                    absorbDamageCheckMsg.Instance = instanceDamage;
                    absorbDamageCheckMsg.Type = msg.Type;
                    _controller.gameObject.SendMessageTo(absorbDamageCheckMsg, _controller.transform.parent.gameObject);
                    var absorbed = damage - instanceDamage.Instance;
                    //TODO: Show absorbed damage

                    damage = instanceDamage.Instance;
                    MessageFactory.CacheMessage(absorbDamageCheckMsg);
                    
                    if (damage > 0)
                    {
                        _currentHealth -= damage;
                        var reportDamageMsg = MessageFactory.GenerateReportDamageMsg();
                        reportDamageMsg.Amount = damage;
                        reportDamageMsg.Owner = msg.Owner && msg.Owner != _controller.transform.parent.gameObject ? msg.Owner : null;
                        _controller.gameObject.SendMessageTo(reportDamageMsg, _controller.transform.parent.gameObject);
                        if (msg.Owner)
                        {
                            _controller.gameObject.SendMessageTo(reportDamageMsg, msg.Owner);
                        }
                        MessageFactory.CacheMessage(reportDamageMsg);
                        UpdateHealth();

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
                    }
                }
                else
                {
                    //TODO: Show resisted
                }
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
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

        private void ReportDamage(ReportDamageMessage msg)
        {
            if (_battleState != UnitBattleState.Dead && msg.Owner && msg.Owner == _controller.transform.parent.gameObject)
            {
                var mana = BattleLeagueManager.GetManaFromDamageDone(msg.Amount);
                _currentMana = Mathf.Min(mana + _currentMana, _baseStats.Mana + _bonusStats.Mana);
                UpdateMana();
                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void UpdateUnitBattleState(UpdateUnitBattleStateMessage msg)
        {
            _battleState = msg.State;
        }

        private void QueryHealth(QueryHealthMessage msg)
        {
            msg.DoAfter.Invoke(_currentHealth, _baseStats.Health + _bonusStats.Health);
        }

        private void QueryMana(QueryManaMessage msg)
        {
            msg.DoAfter.Invoke((int) _currentMana, _baseStats.Mana + _bonusStats.Mana);
        }

        private void ApplyMana(ApplyManaMessage msg)
        {
            var mana = _currentMana + msg.Amount;
            _currentMana = Mathf.Min(Mathf.Max(0, mana), _baseStats.Mana + _bonusStats.Mana);
            UpdateMana();
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void QueryCombatStats(QueryCombatStatsMessage msg)
        {
            msg.DoAfter.Invoke(_baseStats, _bonusStats, GeneticCombatStats.Zero);
        }

        private void QueryBonusDamage(QueryBonusDamageMessage msg)
        {
            var bonusDamage = WorldCombatController.CalculateDamage(_baseStats + _bonusStats, msg.Type);
            msg.DoAfter.Invoke(bonusDamage);
        }

        private void QueryBonusHeal(QueryBonusHealMessage msg)
        {
            var bonusHeal = WorldCombatController.CalculateHeal(_baseStats + _bonusStats, msg.Type);
            msg.DoAfter.Invoke(bonusHeal);
        }
    }
}