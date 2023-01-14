using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEditor;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler")]
    public class HobblerTrait : Trait
    {
        private string _name = string.Empty;
        private HobblerTemplate _template = null;
        private string _hobblerId = string.Empty;
        private HobblerBattleHistory _battleHistory = new HobblerBattleHistory();

        private MonsterState _monsterState = MonsterState.Idle;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
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
        }

        private void SetHobblerTemplate(SetHobblerTemplateMessage msg)
        {
            _template = msg.Template;
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
                data.Stats = stats + bonus + genetics;
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
            _battleHistory.ApplyUnitData(msg.Data, msg.Result);
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
    }
}