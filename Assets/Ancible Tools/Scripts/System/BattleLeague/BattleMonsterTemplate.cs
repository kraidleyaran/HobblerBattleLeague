using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [CreateAssetMenu(fileName = "Battle Monster Template", menuName = "Ancible Tools/Battle/Monster Template")]
    public class BattleMonsterTemplate : ScriptableObject
    {
        public BattleUnitData Data = null;
        public Vector2Int[] PreferredPositions = new Vector2Int[0];
        public MinigameCombatStatsTrait MinigameStats = null;
        public BasicAttackTrait BasicAttack = null;

        public Vector2Int[] GetRelativePreferredPositions(Vector2Int offset)
        {
            var returnValues = new Vector2Int[PreferredPositions.Length];
            for (var i = 0; i < PreferredPositions.Length; i++)
            {
                returnValues[i] = offset + PreferredPositions[i];
            }

            return returnValues;
        }

        public BattleUnitData GetData()
        {
            var data = Data.Clone();
            if (MinigameStats)
            {
                data.Stats = MinigameStats.StartingStats;
            }

            if (BasicAttack)
            {
                data.BasicAttack = BasicAttack.AttackSetup;
            }

            return data;
        }
        
    }
}