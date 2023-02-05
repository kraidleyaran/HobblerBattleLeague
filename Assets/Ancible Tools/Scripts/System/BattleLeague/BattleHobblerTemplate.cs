using System;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [Serializable]
    public class BattleHobblerTemplate
    {
        public HobblerTemplate Hobbler = null;
        public IntNumberRange LevelRange = IntNumberRange.One;
        public IntNumberRange GeneticsBonusRoll = IntNumberRange.One;
        public Vector2Int[] PreferredPositions = new Vector2Int[0];
        public AiAbility[] Abilties = new AiAbility[0];
        public WeaponItem Weapon = null;

        public Vector2Int[] GetRelativePreferredPositions(Vector2Int offset)
        {
            var returnValues = new Vector2Int[PreferredPositions.Length];
            for (var i = 0; i < PreferredPositions.Length; i++)
            {
                returnValues[i] = offset + PreferredPositions[i];
            }

            return returnValues.ToArray();
        }

        public BattleUnitData GenerateBattleUnitData()
        {
            var genetics = Hobbler.GenerateGeneticStats() * LevelRange.Roll();
            var stats = Hobbler.StartingStats;

            return new BattleUnitData
            {
                Name = Hobbler.DisplayName,
                Abilities = Abilties.OrderByDescending(a => a.Priority).Select(a => a.Ability).ToArray(),
                BasicAttack = Weapon?.AttackSetup,
                EquippedItems = new EquippableItem[0],
                Sprite = Hobbler.Sprite,
                Stats = stats + genetics,
            };
        }
    }
}