using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [CreateAssetMenu(fileName = "World Skill", menuName = "Ancible Tools/World Skill")]
    public class WorldSkill : ScriptableObject
    {
        public string DisplayName;
        public string Verb;
        [TextArea(1, 5)] public string Description;
        public Sprite Icon;
        public float ExperienceMultiplier;
        public int LevelExperience;
        public SkillLevel[] Levels = new SkillLevel[0];
        public UnitCommand Command = null;
        public ResourceItem[] Items = new ResourceItem[0];
    }
}