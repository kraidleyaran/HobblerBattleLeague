using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague
{
    [CreateAssetMenu(fileName = "Battle Monster Template", menuName = "Ancible Tools/Battle/Monster Template")]
    public class BattleMonsterTemplate : ScriptableObject
    {
        public BattleUnitData Data = null;
        public Vector2Int[] PreferredPositions = new Vector2Int[0];

        public Vector2Int[] GetRelativePreferredPositions(Vector2Int offset)
        {
            var returnValues = new Vector2Int[PreferredPositions.Length];
            for (var i = 0; i < PreferredPositions.Length; i++)
            {
                returnValues[i] = offset + PreferredPositions[i];
            }

            return returnValues;
        }
    }
}