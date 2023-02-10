using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [CreateAssetMenu(fileName = "World Crafting Skill", menuName = "Ancible Tools/Skills/Crafting Skill")]
    public class WorldCraftingSkill : WorldSkill
    {
        public override WorldSkillType SkillType => WorldSkillType.Crafting;
    }
}