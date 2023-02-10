using Assets.Ancible_Tools.Scripts.System.WorldNodes;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Resource Node Trait", menuName = "Ancible Tools/Traits/Node/Resource Node")]
    public class ResourceNodeTrait : WorldNodeTrait
    {
        [SerializeField] private ItemStack _item;
        [SerializeField] private WorldGatheringSkill _skill = null;
        [SerializeField] private float _skillBonusPercent = 1f;

        protected internal override void ApplyToUnit(GameObject obj)
        {
            base.ApplyToUnit(obj);
            WorldStashController.AddItem(_item.Item, _item.Stack);
        }

        protected internal override void RegisterNode(MapTile tile)
        {
            _registeredNode = WorldNodeManager.RegisterResourceNode(_controller.transform.parent.gameObject, tile, new[]{_item.Item});;
        }

        protected internal override void UnregisterNode()
        {
            WorldNodeManager.UnregisterNode(_controller.transform.parent.gameObject, WorldNodeType.Resource);
            _registeredNode = null;
        }

        protected internal override int GetRequiredTicks(GameObject owner)
        {
            var bonus = 0f;
            var querySkillBonusMsg = MessageFactory.GenerateQuerySkillBonusMsg();
            querySkillBonusMsg.DoAfter = skillBonus => bonus = skillBonus * _skillBonusPercent;
            querySkillBonusMsg.Skill = _skill;
            _controller.gameObject.SendMessageTo(querySkillBonusMsg, owner);
            MessageFactory.CacheMessage(querySkillBonusMsg);

            var tickBonus = Mathf.RoundToInt(bonus);
            var ticks = _requiredTicks - tickBonus;
            if (ticks <= 0)
            {
                ticks = 1;
            }
            return ticks;
        }
    }
}