using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [CreateAssetMenu(fileName = "Monster Battle Encounter", menuName = "Ancible Tools/Battle/Encounter/Monster Encounter")]
    public class MonsterBattleEncounter : BattleEncounter
    {
        public MonsterEncounter[] Monsters = new MonsterEncounter[0];
        public override int TotalUnits => Monsters.Length;
        

        public override KeyValuePair<MapTile, int>[] GetBattleUnits(Vector2Int min, MapTile[] available)
        {
            var returnUnits = new Dictionary<MapTile, int>();
            var availablePositions = available.ToList();
            var monsterPool = new List<int>();
            for (var i = 0; i < Monsters.Length; i++)
            {
                var prio = Monsters[i].Priority;
                monsterPool.Add(i);
                var count = 0;
                while (count < prio)
                {
                    monsterPool.Add(i);
                    count++;
                }
            }

            for (var i = 0; i < MaxPlayableMonsters && monsterPool.Count > 0; i++)
            {
                var monsterIndex = monsterPool.GetRandom();
                monsterPool.RemoveAll(index => index == monsterIndex);
                var positions = Monsters[monsterIndex].Template.GetRelativePreferredPositions(min).Select(p => availablePositions.FirstOrDefault(t => t.Position == p)).Where(p => p != null).ToArray();
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
            for (var i = 0; i < Monsters.Length; i++)
            {
                returnData.Add(Monsters[i].Template.GetData());
            }

            return returnData.ToArray();
        }
    }
}