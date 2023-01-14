using System;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [Serializable]
    public class BattleUnitData : IDisposable
    {
        public string Name;
        [HideInInspector] public string Id;
        public CombatStats Stats = new CombatStats();
        public WorldAbility[] Abilities = new WorldAbility[0];
        public EquippableItem[] EquippedItems = new EquippableItem[0];
        public SpriteTrait Sprite = null;
        public BasicAttackSetup BasicAttack = null;
        public int TotalDamageDone;
        public int TotalHeals;
        public int TotalDamageTaken;
        public int Deaths;
        public int RoundsPlayed;

        public BattleUnitData Clone()
        {
            return new BattleUnitData
            {
                Name = Name,
                Stats = Stats,
                Abilities = Abilities.ToArray(),
                EquippedItems = EquippedItems.ToArray(),
                Sprite = Sprite,
                BasicAttack = BasicAttack
            };
        }

        public void Dispose()
        {
            Name = null;
            Id = null;
            Stats = CombatStats.Zero;
            Abilities = null;
            EquippedItems = null;
            Sprite = null;
            BasicAttack = null;
            TotalDamageDone = 0;
            TotalHeals = 0;
            TotalDamageTaken = 0;
            RoundsPlayed = 0;
            Deaths = 0;
        }
    }
}