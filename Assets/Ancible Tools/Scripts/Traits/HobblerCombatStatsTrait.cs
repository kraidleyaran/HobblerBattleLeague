using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Combat Stats Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler Combat Stats")]
    public class HobblerCombatStatsTrait : Trait
    {
        [SerializeField] private CombatStats _startingStats  = CombatStats.Zero;
        [SerializeField] private GeneticCombatStats _minGeneticStats = GeneticCombatStats.Zero;
        [SerializeField] private GeneticCombatStats _maxGeneticStats = GeneticCombatStats.Zero;
        

        private CombatStats _baseStats = CombatStats.Zero;
        private CombatStats _bonusStats = CombatStats.Zero;

        private GeneticCombatStats _accumulatedGenetics = GeneticCombatStats.Zero;
        private GeneticCombatStats _rolledGenetics = GeneticCombatStats.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _baseStats = _startingStats;
            SubscribeToMessages();
        }

        private void UpdateStats(bool refresh)
        {

            var updateCombatStatsMsg = MessageFactory.GenerateUpdateCombatStatsMsg();
            updateCombatStatsMsg.Base = _baseStats;
            updateCombatStatsMsg.Bonus = _bonusStats;
            updateCombatStatsMsg.Genetics = _accumulatedGenetics;
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
            _controller.transform.parent.gameObject.SubscribeWithFilter<LevelUpMessage>(LevelUp, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupHobblerCombatStatsMessage>(SetupCombatStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobblerGeneticsMessage>(QueryHobblerGenetics, _instanceId);
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
            msg.DoAfter.Invoke(_baseStats, _bonusStats, _accumulatedGenetics);
        }

        private void SetCombatStats(SetCombatStatsMessage msg)
        {
            _baseStats = msg.Stats;
            _accumulatedGenetics = msg.Accumulated;
            UpdateStats(false);
        }

        private void LevelUp(LevelUpMessage msg)
        {
            _accumulatedGenetics += _rolledGenetics;
            UpdateStats(true);
        }

        private void SetupCombatStats(SetupHobblerCombatStatsMessage msg)
        {
            _baseStats = msg.Stats;
            _rolledGenetics = msg.Genetics;
            _accumulatedGenetics = msg.Accumulated;
        }

        private void QueryHobblerGenetics(QueryHobblerGeneticsMessage msg)
        {
            msg.DoAfter.Invoke(_rolledGenetics, _accumulatedGenetics);
        }
    }
}