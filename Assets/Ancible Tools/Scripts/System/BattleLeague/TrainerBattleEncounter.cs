using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [CreateAssetMenu(fileName = "Trainer Battle Encounter", menuName = "Ancible Tools/Battle/Encounter/Trainer Encounter")]
    public class TrainerBattleEncounter : BattleEncounter
    {
        public override int TotalUnits => Hobblers.Length;

        public BattleHobblerTemplate[] Hobblers = new BattleHobblerTemplate[0];
        public ItemStack[] InitialVictoryLoot = new ItemStack[0];

        public override KeyValuePair<MapTile, int>[] GetBattleUnits(Vector2Int min, MapTile[] available)
        {
            var returnUnits = new Dictionary<MapTile, int>();
            var availablePositions = available.ToList();
            var monsterPool = new List<int>();
            for (var i = 0; i < Hobblers.Length; i++)
            {
                monsterPool.Add(i);
                //var count = 0;
                //while (count < prio)
                //{
                //    monsterPool.Add(i);
                //    count++;
                //}
            }

            for (var i = 0; i < MaxPlayableMonsters && monsterPool.Count > 0; i++)
            {
                var monsterIndex = monsterPool.GetRandom();
                monsterPool.RemoveAll(index => index == monsterIndex);
                var positions = Hobblers[monsterIndex].GetRelativePreferredPositions(min).Select(p => availablePositions.FirstOrDefault(t => t.Position == p)).Where(p => p != null).ToArray();
                MapTile mapTile = null;
                if (positions.Length > 0)
                {
                    mapTile = positions.GetRandom();
                    availablePositions.Remove(mapTile);
                }
                else if (availablePositions.Count > 0)
                {
                    mapTile = availablePositions.GetRandom();
                    availablePositions.Remove(mapTile);
                }

                if (mapTile != null)
                {
                    returnUnits.Add(mapTile, monsterIndex);
                }
            }

            return returnUnits.ToArray();
        }

        public override BattleUnitData[] GenerateInstances()
        {
            var returnData = new List<BattleUnitData>();
            foreach (var hobbler in Hobblers)
            {
                returnData.Add(hobbler.GenerateBattleUnitData());
            }

            return returnData.ToArray();
        }

        public override ItemStack[] GenerateLoot(bool repeatable)
        {
            return repeatable ? base.GenerateLoot(true) : InitialVictoryLoot.ToArray();
        }
    }
}