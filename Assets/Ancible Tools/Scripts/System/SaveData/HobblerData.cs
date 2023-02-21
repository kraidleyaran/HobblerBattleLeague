using System;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class HobblerData
    {
        public const string EXTENSION = "hmd";

        public string Name = string.Empty;
        public Vector2IntData Position;
        public string Template = string.Empty;
        public string Id = string.Empty;
        public bool Roster;
        public WellbeingStats Wellbeing = WellbeingStats.Zero;
        public WellbeingStats MaxWellbeing = WellbeingStats.Zero;
        public CombatStats Stats = CombatStats.Zero;
        public GeneticCombatStats Genetics = GeneticCombatStats.Zero;
        public GeneticCombatStats Accumulated = GeneticCombatStats.Zero;
        public AbilityData[] Abilities = new AbilityData[0];
        public SkillData[] Skills = new SkillData[0];
        public EquippableItemData[] Equipped = new EquippableItemData[0];
        public HobblerBattleHistory[] BattleHistory = new HobblerBattleHistory[0];
        public int Level = 0;
        public int Experience = 0;
        public int ExperiencePool = 0;

        public void Dispose()
        {
            Name = null;
            Position = Vector2IntData.Zero;
            Template = null;
            Id = null;
            Roster = false;
            Wellbeing = WellbeingStats.Zero;
            Stats = CombatStats.Zero;
            Genetics = GeneticCombatStats.Zero;
            Abilities = null;
            Skills = null;
            Equipped = null;
            BattleHistory = null;
            Level = 0;
            Experience = 0;
            ExperiencePool = 0;
        }
    }
}