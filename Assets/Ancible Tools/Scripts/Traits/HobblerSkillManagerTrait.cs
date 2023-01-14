using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
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
        

        private Dictionary<WorldSkill, SkillInstance> _skills = new Dictionary<WorldSkill, SkillInstance>();
        private Dictionary<int, SkillInstance> _prioritizedSkills = new Dictionary<int, SkillInstance>();

        private MapTile _currentTile = null;

        private CommandInstance _workInstance = null;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            //var workCommand = Instantiate(_workCommand, _controller.transform);
            _workInstance = _workCommand.GenerateInstance();
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

        private CommandInstance GenerateSkillCommand(SkillInstance instance)
        {
            var skillCommand = Instantiate(FactoryController.COMMAND_TEMPLATE, _controller.transform);
            skillCommand.Icons = new[] { new CommandIcon { Sprite = instance.Instance.Icon, ColorMask = Color.white } };
            skillCommand.Command = instance.Instance.Verb;
            skillCommand.Description = instance.Instance.Description;
            skillCommand.DoAfter = () => { };
            var skillCommandInstance = skillCommand.GenerateInstance();
            var items = instance.Instance.Items;
            for (var it = 0; it < items.Length; it++)
            {
                var item = items[it];
                var itemCommand = Instantiate(FactoryController.COMMAND_TEMPLATE, _controller.transform);
                itemCommand.Icons = new[] { new CommandIcon { Sprite = item.Icon, ColorMask = Color.white } };
                itemCommand.DoAfter = () => { SearchForResourceNode(item); };
                itemCommand.Command = $"{instance.Instance.Verb} {item.DisplayName}";
                var itemInstance = itemCommand.GenerateInstance();
                skillCommandInstance.Tree.SubCommands.Add(itemInstance);
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
            }
            _controller.gameObject.SendMessageTo(RefreshUnitMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void SkillCheck(SkillCheckMessage msg)
        {
            for (var i = 0; i < _prioritizedSkills.Count; i++)
            {
                var skill = _prioritizedSkills[i];
                var items = skill.Instance.Items.Where(it => it.RequiredLevel <= skill.Level && WorldNodeManager.IsResourceAvailable(it)).ToArray();
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
    }
}