using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Ai Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler Ai")]
    public class HobblerAiTrait : Trait
    {
        private HobblerAiState _aiState = HobblerAiState.Auto;
        private MonsterState _monsterState = MonsterState.Idle;
        private Dictionary<WorldNodeType, float> _needs = new Dictionary<WorldNodeType, float>();

        private bool _fulfillingNeed = false;
        private HappinessState _happinessState = HappinessState.Moderate;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.gameObject.SubscribeWithFilter<UpdateTickMessage>(UpdateTick);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateWellbeingMessage>(UpdateWellbeing, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHobblerAiStateMessage>(SetHobblerAiState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateHappinessMessage>(UpdateHappiness, _instanceId);
        }

        private void UpdateWellbeing(UpdateWellbeingMessage msg)
        {
            var percents = (msg.Max - msg.Stats) / msg.Max;
            if (percents.Boredom <= WellBeingController.WarningPercent)
            {
                if (_needs.ContainsKey(WorldNodeType.Activity))
                {
                    _needs[WorldNodeType.Activity] = msg.Stats.Boredom;
                }
                else
                {
                    _needs.Add(WorldNodeType.Activity, msg.Stats.Boredom);
                }
            }
            else
            {
                if (_needs.ContainsKey(WorldNodeType.Activity))
                {
                    _needs.Remove(WorldNodeType.Activity);
                }
                
                if (_monsterState == MonsterState.Gathering && _happinessState == HappinessState.Unhappy)
                {
                    _fulfillingNeed = false;
                    var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                    setMonsterStateMsg.State = MonsterState.Idle;
                    _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMonsterStateMsg);
                }
            }

            if (percents.Fatigue <= WellBeingController.WarningPercent)
            {
                if (_needs.ContainsKey(WorldNodeType.Bed))
                {
                    _needs[WorldNodeType.Bed] = msg.Stats.Fatigue;
                }
                else
                {
                    _needs.Add(WorldNodeType.Bed, msg.Stats.Fatigue);
                }
            }
            else
            {
                if (_needs.ContainsKey(WorldNodeType.Bed))
                {
                    _needs.Remove(WorldNodeType.Bed);
                }
                
                if (_monsterState == MonsterState.Resting)
                {
                    _fulfillingNeed = false;
                    var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                    setMonsterStateMsg.State = MonsterState.Idle;
                    _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMonsterStateMsg);
                }
            }


            if (percents.Ignorance < WellBeingController.WarningPercent)
            {
                if (_needs.ContainsKey(WorldNodeType.Book))
                {
                    _needs[WorldNodeType.Book] = msg.Stats.Ignorance;
                }
                else
                {
                    _needs.Add(WorldNodeType.Book, msg.Stats.Ignorance);
                }
                
            }
            else
            {
                if (_needs.ContainsKey(WorldNodeType.Book))
                {
                    _needs.Remove(WorldNodeType.Book);
                }
                
                if (_monsterState == MonsterState.Studying)
                {
                    _fulfillingNeed = false;
                    var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                    setMonsterStateMsg.State = MonsterState.Idle;
                    _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMonsterStateMsg);
                }
            }

            if (percents.Hunger <= WellBeingController.WarningPercent)
            {
                if (_needs.ContainsKey(WorldNodeType.Food))
                {
                    _needs[WorldNodeType.Food] = msg.Stats.Hunger;
                }
                else
                {
                    _needs.Add(WorldNodeType.Food, msg.Stats.Hunger);
                }
                
            }
            else
            {
                if(_needs.ContainsKey(WorldNodeType.Food))
                {
                    _needs.Remove(WorldNodeType.Food);
                }
                
                if (_monsterState == MonsterState.Eating)
                {
                    _fulfillingNeed = false;
                    var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                    setMonsterStateMsg.State = MonsterState.Idle;
                    _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setMonsterStateMsg);
                }
            }
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (!_fulfillingNeed)
            {
                if (_aiState == HobblerAiState.Command && _happinessState == HappinessState.Unhappy)
                {
                    _aiState = HobblerAiState.Auto;
                }
                if (_aiState != HobblerAiState.Command)
                {
                    if (_needs.Count > 0)
                    {
                        var orderedNeeds = _needs.OrderByDescending(kv => kv.Value).ToArray();
                        var needIndex = 0;
                        var searchForNodeMsg = MessageFactory.GenerateSearchForNodeMsg();
                        searchForNodeMsg.DoAfter = () => { _fulfillingNeed = true; };
                        while (!_fulfillingNeed && needIndex < orderedNeeds.Length)
                        {
                            var need = orderedNeeds[needIndex];
                            switch (need.Key)
                            {
                                case WorldNodeType.Food:
                                    if (_monsterState != MonsterState.Eating)
                                    {
                                        searchForNodeMsg.Type = WorldNodeType.Food;
                                    }
                                    break;
                                case WorldNodeType.Bed:
                                    if (_monsterState != MonsterState.Resting)
                                    {
                                        searchForNodeMsg.Type = WorldNodeType.Bed;
                                    }
                                    break;
                                case WorldNodeType.Book:
                                    if (_monsterState != MonsterState.Studying)
                                    {
                                        searchForNodeMsg.Type = WorldNodeType.Book;
                                    }
                                    break;
                                case WorldNodeType.Activity:
                                    if (_monsterState != MonsterState.Minigame && _monsterState != MonsterState.Gathering)
                                    {
                                        searchForNodeMsg.Type = WorldNodeType.Activity;
                                    }
                                    break;
                            }
                            _controller.gameObject.SendMessageTo(searchForNodeMsg, _controller.transform.parent.gameObject);
                            needIndex++;
                        }
                        MessageFactory.CacheMessage(searchForNodeMsg);
                        if (_fulfillingNeed)
                        {
                            _aiState = HobblerAiState.Auto;
                        }
                    }
                    else if (_monsterState != MonsterState.Gathering && _monsterState != MonsterState.Minigame && _monsterState != MonsterState.Battle)
                    {
                        var activeSkill = false;
                        var skillCheckMsg = MessageFactory.GenerateSkillCheckMsg();
                        skillCheckMsg.DoAfter = () => { activeSkill = true; };
                        _controller.gameObject.SendMessageTo(skillCheckMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(skillCheckMsg);
                        if (!activeSkill)
                        {
                            var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                            setMonsterStateMsg.State = MonsterState.Idle;
                            _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(setMonsterStateMsg);
                        }
                    }
                }
                else if (_aiState == HobblerAiState.Auto && !_fulfillingNeed)
                {
                    switch (_monsterState)
                    {
                        case MonsterState.Idle:
                        case MonsterState.Studying:
                        case MonsterState.Eating:
                        case MonsterState.Resting:
                            var activeSkill = false;
                            var skillCheckMsg = MessageFactory.GenerateSkillCheckMsg();
                            skillCheckMsg.DoAfter = () => { activeSkill = true; };
                            _controller.gameObject.SendMessageTo(skillCheckMsg, _controller.transform.parent.gameObject);
                            MessageFactory.CacheMessage(skillCheckMsg);
                            if (!activeSkill && _monsterState != MonsterState.Idle)
                            {
                                var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                                setMonsterStateMsg.State = MonsterState.Idle;
                                _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                                MessageFactory.CacheMessage(setMonsterStateMsg);
                            }
                            break;
                    }
                }
            }

        }

        private void SetHobblerAiState(SetHobblerAiStateMessage msg)
        {
            _aiState = msg.State;
            if (_aiState == HobblerAiState.Command && _fulfillingNeed)
            {
                _fulfillingNeed = false;
            }
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            _monsterState = msg.State;
            if (_monsterState == MonsterState.Idle && _fulfillingNeed)
            {
                _fulfillingNeed = false;
            }
        }

        private void UpdateHappiness(UpdateHappinessMessage msg)
        {
            _happinessState = msg.State;
        }
    }
}