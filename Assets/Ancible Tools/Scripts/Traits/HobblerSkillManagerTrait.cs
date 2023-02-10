using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Skill Manager Trait", menuName = "Ancible Tools/Traits/Hobbler/Skills/Hobbler Skill Manager")]
    public class HobblerSkillManagerTrait : Trait
    {
        [SerializeField] private WorldSkill[] _startingSkills = new WorldSkill[0];
        [SerializeField] private UnitCommand _workCommand = null;
        [SerializeField] private UnitCommand _craftCommand = null;

        private Dictionary<WorldSkill, SkillInstance> _skills = new Dictionary<WorldSkill, SkillInstance>();
        private Dictionary<int, SkillInstance> _prioritizedSkills = new Dictionary<int, SkillInstance>();

        private MapTile _currentTile = null;

        private CommandInstance _workInstance = null;
        private CommandInstance _craftInstance = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            //var workCommand = Instantiate(_workCommand, _controller.transform);
            _workInstance = _workCommand.GenerateInstance();
            _craftInstance = _craftCommand.GenerateInstance();
            _workInstance.Tree.SubCommands.Add(_craftInstance);
            for (var i = 0; i < _startingSkills.Length; i++)
            {
                if (!_skills.ContainsKey(_startingSkills[i]))
                {
                    var instance = new SkillInstance(_startingSkills[i]);
                    _skills.Add(instance.Instance, instance);
                    _prioritizedSkills.Add(_prioritizedSkills.Count, instance);

                    _workInstance.Tree.SubCommands.Add(GenerateSkillCommand(instance));
                }
            }

            SubscribeToMessages();
        }

        private void SearchForResourceNode(WorldItem item)
        {
            var searchForResourceNodeMsg = MessageFactory.GenerateSearchForResourceNodeMsg();
            searchForResourceNodeMsg.Item = item;
            _controller.gameObject.SendMessageTo(searchForResourceNodeMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(searchForResourceNodeMsg);
        }

        private void SearchForCraftingNode(WorldCraftingSkill skill)
        {
            var searchForCraftingNodeMsg = MessageFactory.GenerateSearchForCraftingNodeMsg();
            searchForCraftingNodeMsg.Skill = skill;
            searchForCraftingNodeMsg.DoAfter = null;
            _controller.gameObject.SendMessageTo(searchForCraftingNodeMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(searchForCraftingNodeMsg);
        }

        private CommandInstance GenerateSkillCommand(SkillInstance instance)
        {
            var skillCommand = Instantiate(FactoryController.COMMAND_TEMPLATE, _controller.transform);
            skillCommand.Icons = new[] { new CommandIcon { Sprite = instance.Instance.Icon, ColorMask = Color.white } };
            skillCommand.Command = instance.Instance.Verb;
            skillCommand.Description = instance.Instance.Description;
            skillCommand.DoAfter = () => { };
            var skillCommandInstance = skillCommand.GenerateInstance();
            if (instance.Instance.SkillType == WorldSkillType.Gathering && instance.Instance is WorldGatheringSkill gathering)
            {
                
                var items = gathering.Items;
                foreach (var item in items)
                {
                    //var item = items[it];
                    var itemCommand = Instantiate(FactoryController.COMMAND_TEMPLATE, _controller.transform);
                    itemCommand.Icons = new[] { new CommandIcon { Sprite = item.Icon, ColorMask = Color.white } };
                    itemCommand.DoAfter = () => { SearchForResourceNode(item); };
                    itemCommand.Command = $"{instance.Instance.Verb} {item.DisplayName}";
                    var itemInstance = itemCommand.GenerateInstance();
                    skillCommandInstance.Tree.SubCommands.Add(itemInstance);
                }
            }
            else if (instance.Instance.SkillType == WorldSkillType.Crafting && instance.Instance is WorldCraftingSkill craftingSkill)
            {
                var craftingCommand = Instantiate(FactoryController.COMMAND_TEMPLATE, _controller.transform);
                craftingCommand.Icons = new[] {new CommandIcon {Sprite = craftingSkill.Icon},};
                craftingCommand.DoAfter = () => { SearchForCraftingNode(craftingSkill); };
                craftingCommand.Command = $"{craftingSkill.DisplayName}";
                var craftingInstance = craftingCommand.GenerateInstance();
                _craftInstance.Tree.SubCommands.Add(craftingInstance);
            }

            return skillCommandInstance;
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<GainSkillExperienceMessage>(GainSkillExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SkillCheckMessage>(SkillCheck, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QuerySkillsByPriorityMessage>(QuerySkillsByPriority, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ChangeSkillPriorityMessage>(ChangeSkillPriority, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetSkillsFromDataMessage>(SetSkillsFromData, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ApplySkillBonusMessage>(ApplySkillBonus, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QuerySkillBonusMessage>(QuerySkillBonus, _instanceId);
        }

        private void GainSkillExperience(GainSkillExperienceMessage msg)
        {
            if (!_skills.TryGetValue(msg.Skill, out var instance))
            {
                instance = new SkillInstance(msg.Skill);
                _skills.Add(msg.Skill, instance);
                _prioritizedSkills.Add(_prioritizedSkills.Count, instance);
                _workInstance.Tree.SubCommands.Add(GenerateSkillCommand(instance));
                _controller.gameObject.SendMessageTo(ResetCommandCardMessage.INSTANCE, _controller.transform.parent.gameObject);
            }

            var levelsGained = instance.GainExperience(msg.Experience);
            if (levelsGained > 0)
            {
                instance.ApplyLevels(levelsGained, _controller.transform.parent.gameObject);
                UiAlertManager.ShowAlert($"+{levelsGained} {msg.Skill.DisplayName}", msg.Skill.Icon, Color.white);
            }
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void SkillCheck(SkillCheckMessage msg)
        {
            for (var i = 0; i < _prioritizedSkills.Count; i++)
            {
                var skill = _prioritizedSkills[i];
                if (skill.Instance.SkillType == WorldSkillType.Gathering && skill.Instance is WorldGatheringSkill gatheringSkill)
                {
                    var items = gatheringSkill.Items.Where(it => it.RequiredLevel <= skill.Level && WorldNodeManager.IsResourceAvailable(it)).ToArray();
                    if (items.Length > 0)
                    {
                        var orderedItems = items.OrderByDescending(it => it.RequiredLevel).ToArray();
                        var highestLevel = orderedItems[0].RequiredLevel;
                        var item = items.Where(it => it.RequiredLevel == highestLevel).ToArray().GetRandom();
                        var searchForResourceNodeMsg = MessageFactory.GenerateSearchForResourceNodeMsg();
                        searchForResourceNodeMsg.Item = item;
                        _controller.gameObject.SendMessageTo(searchForResourceNodeMsg, _controller.transform.parent.gameObject);
                        MessageFactory.CacheMessage(searchForResourceNodeMsg);
                        msg.DoAfter?.Invoke();
                        break;
                    }
                }
                else if (skill.Instance.SkillType == WorldSkillType.Crafting)
                {
                    var searchForCraftingNodeMsg = MessageFactory.GenerateSearchForCraftingNodeMsg();
                    searchForCraftingNodeMsg.Skill = skill.Instance;
                    _controller.gameObject.SendMessageTo(searchForCraftingNodeMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(searchForCraftingNodeMsg);
                    msg.DoAfter?.Invoke();
                }

            }
        }

        private void QueryCommands(QueryCommandsMessage msg)
        {
            msg.DoAfter.Invoke(new []{_workInstance});
        }

        private void QuerySkillsByPriority(QuerySkillsByPriorityMessage msg)
        {
            msg.DoAfter.Invoke(_prioritizedSkills.ToArray());
        }

        private void ChangeSkillPriority(ChangeSkillPriorityMessage msg)
        {
            if (_prioritizedSkills.TryGetValue(msg.Origin, out var originSkill) && _prioritizedSkills.TryGetValue(msg.Priority, out var toSkill))
            {
                _prioritizedSkills[msg.Origin] = toSkill;
                _prioritizedSkills[msg.Priority] = originSkill;

                _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void SetSkillsFromData(SetSkillsFromDataMessage msg)
        {
            var skills = _skills.ToArray();
            foreach (var skill in skills)
            {
                skill.Value.Destroy();
            }
            _skills.Clear();
            _prioritizedSkills.Clear();
            foreach (var skill in msg.Skills)
            {
                var worldSkill = WorldSkillFactory.GetSkillByName(skill.Skill);
                if (worldSkill)
                {
                    var instance = new SkillInstance(worldSkill, skill);
                    _skills.Add(worldSkill, instance);
                    _prioritizedSkills.Add(skill.Priority, instance);
                }
            }
        }

        private void ApplySkillBonus(ApplySkillBonusMessage msg)
        {
            if (!_skills.TryGetValue(msg.Skill, out var instance))
            {
                instance = new SkillInstance(msg.Skill);
                _skills.Add(msg.Skill, instance);
            }

            if (msg.Permanent)
            {
                instance.Permanent += msg.Bonus;
            }
            else
            {
                instance.Bonus += msg.Bonus;
            }
        }

        private void QuerySkillBonus(QuerySkillBonusMessage msg)
        {
            if (_skills.TryGetValue(msg.Skill, out var instance))
            {
                msg.DoAfter.Invoke(instance.TotalBonus);
            }
        }
    }
}