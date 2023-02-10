using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [CreateAssetMenu(fileName = "Gathering Skill", menuName = "Ancible Tools/Skills/Gathering Skill")]
    public class WorldGatheringSkill : WorldSkill
    {
        public override WorldSkillType SkillType => WorldSkillType.Gathering;
        public ResourceItem[] Items = new ResourceItem[0];
    }
}