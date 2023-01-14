using System;
using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [CreateAssetMenu(fileName = "Hobbler Template", menuName = "Ancible Tools/Hobbler Template")]
    public class HobblerTemplate : ScriptableObject
    {
        public string DisplayName;
        [TextArea(3, 5)] public string Description;
        public SpriteTrait Sprite = null;
        public CombatStats StartingStats = CombatStats.Zero;
        public GeneticCombatStats MinimumGenetics = GeneticCombatStats.Zero;
        public GeneticCombatStats MaximumGenetics = GeneticCombatStats.Zero;
        public WorldAbility[] StartingAbilities = new WorldAbility[0];
        public WeaponItem StartingWeapon = null;
        public int Cost;

        public GeneticCombatStats GenerateGeneticStats()
        {
            var maxRoll = WorldCombatController.MaxGeneticRoll;
            var range = new IntNumberRange {Minimum = 1, Maximum = maxRoll};
            var stats = GeneticCombatStats.Zero;
            stats.Health =  RollStat(MinimumGenetics.Health, MaximumGenetics.Health, range);
            stats.Mana = RollStat(MinimumGenetics.Mana, MaximumGenetics.Mana, range);
            stats.Strength = RollStat(MinimumGenetics.Strength, MaximumGenetics.Strength, range);
            stats.Agility = RollStat(MinimumGenetics.Agility, MaximumGenetics.Agility, range);
            stats.Armor = RollStat(MinimumGenetics.Armor, MaximumGenetics.Armor, range);
            stats.Magic = RollStat(MinimumGenetics.Magic, MaximumGenetics.Magic, range);
            stats.Faith = RollStat(MinimumGenetics.Faith, MaximumGenetics.Faith, range);
            stats.Spirit = MinimumGenetics.Spirit + RollStat(MinimumGenetics.Spirit, MaximumGenetics.Spirit, range, true);
            return stats;
        }

        public CombatStats GetGeneticRolls(GeneticCombatStats genetics)
        {
            var stats = CombatStats.Zero;
            stats.Health = GetGeneticStat(MinimumGenetics.Health, MaximumGenetics.Health, genetics.Health);
            stats.Mana = GetGeneticStat(MinimumGenetics.Mana, MaximumGenetics.Mana, genetics.Mana);
            stats.Strength = GetGeneticStat(MinimumGenetics.Strength, MaximumGenetics.Strength, genetics.Strength);
            stats.Agility = GetGeneticStat(MinimumGenetics.Agility, MaximumGenetics.Agility, genetics.Agility);
            stats.Defense = GetGeneticStat(MinimumGenetics.Armor, MaximumGenetics.Armor, genetics.Armor);
            stats.Magic = GetGeneticStat(MinimumGenetics.Magic, MaximumGenetics.Magic, genetics.Magic);
            stats.Faith = GetGeneticStat(MinimumGenetics.Faith, MaximumGenetics.Faith, genetics.Faith);
            stats.Spirit = GetGeneticStat(MinimumGenetics.Spirit, MaximumGenetics.Spirit, genetics.Spirit, true);
            return stats;
        }

        private static float RollStat(float min, float max, IntNumberRange roll, bool reverse = false)
        {
            if (reverse)
            {
                return max - roll.Roll() * ((max - min) / roll.Maximum) + 1;
            }
            return roll.Roll() * ((max - min) / roll.Maximum);

        }

        private static int GetGeneticStat(float min, float max, float stat, bool reverse = false)
        {
            return WorldCombatController.MaxGeneticRoll - (int) ((max - min) / WorldCombatController.MaxGeneticRoll / stat);
        }

        public string GetDescription()
        {
            return $"{Description}{Environment.NewLine}{Environment.NewLine}{StartingStats.GetDescription(true)}";
        }
    }
}