using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Happiness Trait", menuName = "Ancible Tools/Traits/Stats/Wellbeing/Happiness")]
    public class HappinessTrait : Trait
    {
        [SerializeField] private int _startingHappiness = 5;
        [SerializeField] private IntNumberRange _happinessCaps = new IntNumberRange();
        [SerializeField] private WellbeingStats _minStats = new WellbeingStats();
        [SerializeField] private WellbeingStats _maxStats = new WellbeingStats();

        private WellbeingStats _wellBeingStats = new WellbeingStats();
        
        private Dictionary<WellbeingStatType, TickTimer> _wellbeingTimers = new Dictionary<WellbeingStatType, TickTimer>();
        private MonsterState _monsterState = MonsterState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _wellbeingTimers = new Dictionary<WellbeingStatType, TickTimer>
            {
                { WellbeingStatType.Hunger, new TickTimer(WellBeingController.TicksPerHungerEffect, -1, ApplyHungerEffect, null) },
                { WellbeingStatType.Boredom, new TickTimer(WellBeingController.TicksPerBoredomEffect, -1, ApplyBoredomEffect, null) },
                { WellbeingStatType.Fatigue, new TickTimer(WellBeingController.TicksPerFatigueEffect, -1, ApplyFatigueEffect, null) }
            };
            SubscribeToMessages();
        }

        private void UpdateParent()
        {
            var updateWellbeingMsg = MessageFactory.GenerateUpdateWellbeingMsg();
            updateWellbeingMsg.Stats = _wellBeingStats;
            updateWellbeingMsg.Min = _minStats;
            updateWellbeingMsg.Max = _maxStats;
            _controller.gameObject.SendMessageTo(updateWellbeingMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateWellbeingMsg);

            var updateHappinessMsg = MessageFactory.GenerateUpdateHappinessMsg();
            updateHappinessMsg.Happiness = _wellBeingStats.CalculateHappiness(_happinessCaps);
            _controller.gameObject.SendMessageTo(updateHappinessMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(updateHappinessMsg);

            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void ApplyHungerEffect()
        {
            _wellBeingStats.Hunger += WellBeingController.HungerPerEffect;
            _wellBeingStats.ApplyLimits(_minStats, _maxStats);
            UpdateParent();
        }

        private void ApplyBoredomEffect()
        {
            _wellBeingStats.Boredom += WellBeingController.BoredomPerEffect;
            _wellBeingStats.ApplyLimits(_minStats, _maxStats);

            UpdateParent();
        }

        private void ApplyFatigueEffect()
        {
            _wellBeingStats.Fatigue += WellBeingController.FatiguePerEffect;
            _wellBeingStats.ApplyLimits(_minStats, _maxStats);
            UpdateParent();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHappinessMessage>(QueryHappiness, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyWellbeingStatsMessage>(ApplyWellBeingStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryWellbeingStatsMessage>(QueryWellbeingStats, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobblerWellbeingStatusMessage>(QueryHobblerWellBeingStatus, _instanceId);
        }

        private void QueryHappiness(QueryHappinessMessage msg)
        {
            msg.DoAfter.Invoke(_wellBeingStats.CalculateHappiness(_happinessCaps), _happinessCaps);
        }

        private void ApplyWellBeingStats(ApplyWellbeingStatsMessage msg)
        {
            if (msg.Stats.Hunger < 0)
            {
                _wellbeingTimers[WellbeingStatType.Hunger].Restart();
            }

            if (msg.Stats.Boredom < 0)
            {
                _wellbeingTimers[WellbeingStatType.Boredom].Restart();
            }

            if (msg.Stats.Fatigue < 0)
            {
                _wellbeingTimers[WellbeingStatType.Fatigue].Restart();
            }
            _wellBeingStats += msg.Stats;
            _wellBeingStats.ApplyLimits(_minStats, _maxStats);

            UpdateParent();
        }

        private void QueryWellbeingStats(QueryWellbeingStatsMessage msg)
        {
            msg.DoAfter.Invoke(_wellBeingStats, _minStats, _maxStats);
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            switch (_monsterState)
            {
                case MonsterState.Eating:
                    _wellbeingTimers[WellbeingStatType.Hunger].Play();
                    break;
                case MonsterState.Resting:
                    _wellbeingTimers[WellbeingStatType.Fatigue].Play();
                    break;
                case MonsterState.Minigame:
                    _wellbeingTimers[WellbeingStatType.Hunger].Play();
                    _wellbeingTimers[WellbeingStatType.Fatigue].Play();
                    _wellbeingTimers[WellbeingStatType.Boredom].Play();
                    break;
            }
            switch (msg.State)
            {
                case MonsterState.Idle:
                    break;
                case MonsterState.Eating:
                    _wellbeingTimers[WellbeingStatType.Hunger].Pause();
                    break;
                case MonsterState.Gathering:
                    break;
                case MonsterState.Resting:
                    _wellbeingTimers[WellbeingStatType.Fatigue].Pause();
                    break;
                case MonsterState.Minigame:
                    _wellbeingTimers[WellbeingStatType.Boredom].Pause();
                    _wellbeingTimers[WellbeingStatType.Fatigue].Pause();
                    _wellbeingTimers[WellbeingStatType.Hunger].Pause();
                    break;
            }

            _monsterState = msg.State;
        }

        private void QueryHobblerWellBeingStatus(QueryHobblerWellbeingStatusMessage msg)
        {
            msg.DoAfter.Invoke(_wellBeingStats.GetStatus());
        }
    }
}