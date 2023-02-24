using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEditor;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler")]
    public class HobblerTrait : Trait
    {
        [SerializeField] private UnitCommand _exileCommandTemplate = null;

        private string _name = string.Empty;
        private HobblerTemplate _template = null;
        private string _hobblerId = string.Empty;

        private MapTile _currentMapTile = null;

        private int _currentLevel = 0;
        private int _experience = 0;

        private List<HobblerBattleHistory> _battleHistory = new List<HobblerBattleHistory>();

        private MonsterState _monsterState = MonsterState.Idle;
        private HobblerData _data = null;

        private CommandInstance _exileInstance = null;
        private bool _roster = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var exileCommand = Instantiate(_exileCommandTemplate, _controller.transform);
            _exileInstance = exileCommand.GenerateInstance();
            SubscribeToMessages();
        }

        private void ApplyEquipmentToData(EquippableInstance[] armor, EquippableInstance[] trinkets, EquippableInstance weapon)
        {
            var equipped = new List<EquippableItemData>();
            for (var i = 0; i < armor.Length; i++)
            {
                if (armor[i] != null)
                {
                    equipped.Add(new EquippableItemData{Item = armor[i].Instance.name, Slot = i});
                }
            }

            for (var i = 0; i < trinkets.Length; i++)
            {
                if (trinkets[i] != null)
                {
                    equipped.Add(new EquippableItemData{Item = trinkets[i].Instance.name, Slot = i});
                }
            }

            if (weapon != null)
            {
                equipped.Add(new EquippableItemData{Item = weapon.Instance.name, Slot =  0});
            }

            _data.Equipped = equipped.ToArray();
        }

        private void ApplyCombatStatsToData(CombatStats baseStats, CombatStats bonus, GeneticCombatStats accumulated)
        {
            _data.Stats = baseStats;
        }

        private void ApplyGeneticsToData(GeneticCombatStats rolled, GeneticCombatStats accumulated)
        {
            _data.Genetics = rolled;
            _data.Accumulated = accumulated;
        }

        private void ApplySkillsToData(KeyValuePair<int, SkillInstance>[] skills)
        {
            _data.Skills = skills.Select(kv => kv.ToData()).ToArray();
        }

        private void ApplyWellbeingToData(WellbeingStats wellbeing, WellbeingStats max)
        {
            _data.Wellbeing = wellbeing;
            _data.MaxWellbeing = max;
        }

        private void ApplyAbilitiesToData(KeyValuePair<int, WorldAbility>[] abilities)
        {
            _data.Abilities = abilities.Where(a => a.Value).Select(a => a.ToData()).ToArray();
        }

        private void ApplyExperiencePoolToData(int pool)
        {
            _data.ExperiencePool = pool;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetHobblerTemplateMessage>(SetHobblerTemplate, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetUnitNameMessage>(SetUnitName, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryUnitNameMessage>(QueryUnitName, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobblerIdMessage>(QueryHobblerId, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryBattleUnitDataMessage>(QueryBattleUnitData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplyHobblerBattleDataMessage>(ApplyHobblerBattleData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMonsterStateMessage>(UpdateMonsterState, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobblerDataMessage>(QueryHobblerData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetupHobblerFromDataMessage>(SetupHobblerFromData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateMapTileMessage>(UpdateMapTile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UpdateHobblerExperienceMessage>(UpdateHobblerExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
        }

        private void SetHobblerTemplate(SetHobblerTemplateMessage msg)
        {
            _template = msg.Template;
            _exileInstance.Command.GoldValue = WorldHobblerManager.GetExileGold(_template.Cost);
            if (string.IsNullOrEmpty(msg.Id))
            {
                _hobblerId = GUID.Generate().ToString();
                var genetics = _template.GenerateGeneticStats();
                var setupCombatStatsMsg = MessageFactory.GenerateSetupHobblerCombatStatsMsg();
                setupCombatStatsMsg.Stats = _template.StartingStats;
                setupCombatStatsMsg.Genetics = genetics;
                setupCombatStatsMsg.Accumulated = GeneticCombatStats.Zero;
                _controller.gameObject.SendMessageTo(setupCombatStatsMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setupCombatStatsMsg);

                var setEquipmentMsg = MessageFactory.GenerateSetEquipmentMsg();
                setEquipmentMsg.Items = new EquippableItem[] {_template.StartingWeapon};
                _controller.gameObject.SendMessageTo(setEquipmentMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setEquipmentMsg);

                var setAbilitiesMsg = MessageFactory.GenerateSetAbilitiesMsg();
                setAbilitiesMsg.Abilities = _template.StartingAbilities.ToArray();
                _controller.gameObject.SendMessageTo(setAbilitiesMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setAbilitiesMsg);
            }
            else
            {
                _hobblerId = msg.Id;
            }

            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            addTraitToUnitMsg.Trait = _template.Sprite;
            _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        private void SetUnitName(SetUnitNameMessage msg)
        {
            _name = msg.Name;
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void QueryUnitName(QueryUnitNameMessage msg)
        {
            msg.DoAfter.Invoke(string.IsNullOrEmpty(_name) ? _template.DisplayName : _name);
        }

        private void QueryHobblerId(QueryHobblerIdMessage msg)
        {
            msg.DoAfter.Invoke(_hobblerId);
        }

        private void QueryBattleUnitData(QueryBattleUnitDataMessage msg)
        {
            var data = new BattleUnitData
            {
                Name = _name,
                Id = _hobblerId
            };


            var equippedItems = new List<EquippableItem>();

            var queryEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
            queryEquipmentMsg.DoAfter = (armor, trinkets, weapon) =>
            {
                equippedItems.AddRange(armor.Where(i => i != null).Select(i => i.Instance));
                equippedItems.AddRange(trinkets.Where(i => i != null).Select(i => i.Instance));
                if (weapon != null)
                {
                    equippedItems.Add(weapon.Instance);
                }
            };
            _controller.gameObject.SendMessageTo(queryEquipmentMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryEquipmentMsg);

            data.EquippedItems = equippedItems.ToArray();

            var queryAbilitiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
            queryAbilitiesMsg.DoAfter = abilities => data.Abilities = abilities.Where(kv => kv.Value).OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray();
            _controller.gameObject.SendMessageTo(queryAbilitiesMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryAbilitiesMsg);

            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = (stats, bonus, genetics) =>
            {
                data.Stats = stats + genetics;
            };
            _controller.gameObject.SendMessageTo(queryCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryCombatStatsMsg);

            var queryBasicAttackMsg = MessageFactory.GenerateQueryBasicAttackSetupMsg();
            queryBasicAttackMsg.DoAfter = setup => data.BasicAttack = setup;
            _controller.gameObject.SendMessageTo(queryBasicAttackMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryBasicAttackMsg);

            var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
            querySpriteMsg.DoAfter = trait => data.Sprite = trait;
            _controller.gameObject.SendMessageTo(querySpriteMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(querySpriteMsg);


            
            msg.DoAfter.Invoke(data);
        }

        private void ApplyHobblerBattleData(ApplyHobblerBattleDataMessage msg)
        {
            var history = new HobblerBattleHistory();
            _battleHistory.Add(history.FromBattleData(msg.Data, msg.Result == BattleResult.Victory, msg.MatchId));
            if (_monsterState == MonsterState.Battle)
            {
                var setMonsterStateMsg = MessageFactory.GenerateSetMonsterStateMsg();
                setMonsterStateMsg.State = MonsterState.Idle;
                _controller.gameObject.SendMessageTo(setMonsterStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setMonsterStateMsg);
            }
        }

        private void UpdateMonsterState(UpdateMonsterStateMessage msg)
        {
            if (msg.State == MonsterState.Battle)
            {
                _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                {
                    var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                    setSpriteVisibilityMsg.Visible = false;
                    _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                    var setSelectableStateMsg = MessageFactory.GenerateSetActiveSelectableStateMsg();
                    setSelectableStateMsg.Selectable = false;
                    _controller.gameObject.SendMessageTo(setSelectableStateMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSelectableStateMsg);
                }));
            }
            else if (_monsterState == MonsterState.Battle)
            {
                var setSpriteVisibilityMsg = MessageFactory.GenerateSetSpriteVisibilityMsg();
                setSpriteVisibilityMsg.Visible = true;
                _controller.gameObject.SendMessageTo(setSpriteVisibilityMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setSpriteVisibilityMsg);

                var setSelectableStateMsg = MessageFactory.GenerateSetActiveSelectableStateMsg();
                setSelectableStateMsg.Selectable = true;
                _controller.gameObject.SendMessageTo(setSelectableStateMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(setSelectableStateMsg);
            }
            _monsterState = msg.State;
        }

        private void QueryHobblerData(QueryHobblerDataMessage msg)
        {
            if (_data == null)
            {
                _data = new HobblerData { Id = _hobblerId, Template = _template.name, BattleHistory = _battleHistory.ToArray()};
            }
            //This needs to be outside of the intialization so we always apply the new name;
            _data.Name = _name;
            _data.Roster = WorldHobblerManager.Roster.Contains(_controller.transform.parent.gameObject);
            _data.Position = _currentMapTile.Position.ToData();
            _data.Level = _currentLevel;
            _data.Experience = _experience;

            var queryExperiencePoolMsg = MessageFactory.GenerateQueryExperiencePoolMsg();
            queryExperiencePoolMsg.DoAfter = ApplyExperiencePoolToData;
            _controller.gameObject.SendMessageTo(queryExperiencePoolMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryExperiencePoolMsg);

            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = ApplyCombatStatsToData;
            _controller.gameObject.SendMessageTo(queryCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryCombatStatsMsg);

            var queryHobblerGeneticsMsg = MessageFactory.GenerateQueryHobblerGeneticsMsg();
            queryHobblerGeneticsMsg.DoAfter = ApplyGeneticsToData;
            _controller.gameObject.SendMessageTo(queryHobblerGeneticsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryHobblerGeneticsMsg);

            var queryEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
            queryEquipmentMsg.DoAfter = ApplyEquipmentToData;
            _controller.gameObject.SendMessageTo(queryEquipmentMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryEquipmentMsg);

            var querySkillsMsg = MessageFactory.GenerateQuerySkillsByPriorityMsg();
            querySkillsMsg.DoAfter = ApplySkillsToData;
            _controller.gameObject.SendMessageTo(querySkillsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(querySkillsMsg);

            var queryWellbeingStatsMsg = MessageFactory.GenerateQueryWellbeingStatsMsg();
            queryWellbeingStatsMsg.DoAfter = ApplyWellbeingToData;
            _controller.gameObject.SendMessageTo(queryWellbeingStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryWellbeingStatsMsg);

            var queryAbilitiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
            queryAbilitiesMsg.DoAfter = ApplyAbilitiesToData;
            _controller.gameObject.SendMessageTo(queryAbilitiesMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryAbilitiesMsg);
            
            msg.DoAfter.Invoke(_data);
        }

        private void SetupHobblerFromData(SetupHobblerFromDataMessage msg)
        {
            _data = msg.Data;
            _name = _data.Name;
            _hobblerId = _data.Id;
            _currentLevel = _data.Level;
            _experience = _data.Experience;

            var setupCombatStatsMsg = MessageFactory.GenerateSetupHobblerCombatStatsMsg();
            setupCombatStatsMsg.Stats = _data.Stats;
            setupCombatStatsMsg.Accumulated = _data.Accumulated;
            setupCombatStatsMsg.Genetics = _data.Genetics;
            _controller.gameObject.SendMessageTo(setupCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setupCombatStatsMsg);

            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();

            var template = WorldHobblerManager.GetTemplateByName(_data.Template);
            if (template)
            {
                _template = template;
                addTraitToUnitMsg.Trait = template.Sprite;
                _controller.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
            }


            var setAbilitiesFromDataMsg = MessageFactory.GenerateSetAbilitiesFromDataMsg();
            setAbilitiesFromDataMsg.Abilities = _data.Abilities;
            _controller.gameObject.SendMessageTo(setAbilitiesFromDataMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setupCombatStatsMsg);

            var setSkillsFromDataMsg = MessageFactory.GenerateSetSkillsFromDataMsg();
            setSkillsFromDataMsg.Skills = _data.Skills;
            _controller.gameObject.SendMessageTo(setSkillsFromDataMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setSkillsFromDataMsg);

            var setEquipmentFromDataMsg = MessageFactory.GenerateSetEquippableItemsFromDataMsg();
            setEquipmentFromDataMsg.Items = _data.Equipped;
            _controller.gameObject.SendMessageTo(setEquipmentFromDataMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setEquipmentFromDataMsg);

            var setHobblerExperienceMsg = MessageFactory.GenerateSetHobblerExperienceMsg();
            setHobblerExperienceMsg.Experience = _experience;
            setHobblerExperienceMsg.Level = _currentLevel;
            setHobblerExperienceMsg.Pool = _data.ExperiencePool;
            _controller.gameObject.SendMessageTo(setHobblerExperienceMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setHobblerExperienceMsg);

            var setWellbeingMsg = MessageFactory.GenerateSetWellbeingStatsMsg();
            setWellbeingMsg.Stats = _data.Wellbeing;
            setWellbeingMsg.Maximum = _data.MaxWellbeing;
            _controller.gameObject.SendMessageTo(setWellbeingMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setWellbeingMsg);
        }

        private void UpdateMapTile(UpdateMapTileMessage msg)
        {
            _currentMapTile = msg.Tile;
        }

        private void UpdateHobblerExperience(UpdateHobblerExperienceMessage msg)
        {
            if (_currentLevel != msg.Level)
            {
                if (_template.Evolution.Template && _template.Evolution.RequiredLevel <= _currentLevel)
                {
                    _template = _template.Evolution.Template;
                    var setSpriteTraitMsg = MessageFactory.GenerateSetSpriteMsg();
                    setSpriteTraitMsg.Sprite = _template.Sprite;
                    _controller.gameObject.SendMessageTo(setSpriteTraitMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(setSpriteTraitMsg);
                }


                //TODO: Set combat stats? Available abilities? Unlock Talent tier?

                _currentLevel = msg.Level;
            }

            _experience = msg.Experience;
        }

        private void QueryCommands(QueryCommandsMessage msg)
        {
            msg.DoAfter.Invoke(new []{_exileInstance});
        }
    }
}