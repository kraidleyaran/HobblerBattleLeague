using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    public class BattleEncounter : ScriptableObject
    {
        public int MaxPlayableMonsters = 1;
        public int RequiredPoints = 30;
        public int MaximumRounds = 0;
        public float VictoryExperiencePerPoint;
        public float DefeatExperiencePerPoint;
        public LootTable RepeatableLoot = null;
        public virtual int TotalUnits => 0;
        public bool Save;
        public int GoldRemoveOnDefeat = 0;

        public virtual KeyValuePair<MapTile, int>[] GetBattleUnits(Vector2Int min, MapTile[] available)
        {
            return new KeyValuePair<MapTile, int>[0];
        }

        public virtual BattleUnitData[] GenerateInstances()
        {
            return new BattleUnitData[0];
        }

        public virtual ItemStack[] GenerateLoot(bool repeatable)
        {
            if (RepeatableLoot)
            {
                return RepeatableLoot.GenerateLoot();
            }
            return new ItemStack[0];
        }
    }
}