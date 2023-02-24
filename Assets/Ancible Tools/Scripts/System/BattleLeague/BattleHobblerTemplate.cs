using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
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
        public WeaponItem[] WeaponPool = new WeaponItem[0];
        public EquippableItem[] OtherEquippablesPool = new EquippableItem[0];

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
            var genetics = Hobbler.GenerateGeneticStats(GeneticsBonusRoll.Roll()) * LevelRange.Roll();
            var stats = Hobbler.StartingStats;
            var equipped = new List<EquippableItem>();
            var armor = OtherEquippablesPool.Where(e => e.Slot == EquipSlot.Armor).ToList();
            for (var i = 0; i < WorldItemFactory.MaxArmorSlots && i < armor.Count; i++)
            {
                var item = armor.GetRandom();
                armor.Remove(item);
                equipped.Add(item);
            }

            var trinkets = OtherEquippablesPool.Where(e => e.Slot == EquipSlot.Trinket).ToList();
            for (var i = 0; i < WorldItemFactory.MaxTrinketSlots && i < trinkets.Count; i++)
            {
                var item = trinkets.GetRandom();
                trinkets.Remove(item);
                equipped.Add(item);
            }

            equipped.Add(WeaponPool.Length > 0 ? WeaponPool.GetRandom() : Hobbler.StartingWeapon);

            var abilities = new List<AiAbility>();
            if (Abilties.Length > 0)
            {
                var availableAbilities = Abilties.ToList();
                for (var i = 0; i < DataController.MaxHobblerAbilities && i < availableAbilities.Count; i++)
                {
                    var ability = availableAbilities.GetRandom();
                    availableAbilities.Remove(ability);
                    abilities.Add(ability);
                }
            }

            return new BattleUnitData
            {
                Name = Hobbler.DisplayName,
                Abilities = abilities.Count > 0 ? abilities.OrderByDescending(a => a.Priority).Select(a => a.Ability).ToArray() : Hobbler.StartingAbilities.ToArray(),
                BasicAttack = Hobbler.StartingWeapon.AttackSetup,
                EquippedItems = equipped.ToArray(),
                Sprite = Hobbler.Sprite,
                Stats = stats + genetics,
            };
        }
    }
}