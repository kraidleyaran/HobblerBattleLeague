using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.UI.UnitInfo.Buildings;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Building;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Building
{
    [CreateAssetMenu(fileName = "World Building", menuName = "Ancible Tools/World Building")]
    public class WorldBuilding : ScriptableObject
    {
        public string DisplayName;
        [TextArea(1, 5)] public string Description;
        public Sprite Icon;
        public SpriteTrait[] Sprites;
        public Vector2Int[] RequiredTiles;
        public Vector2Int[] BlockingTiles;
        public UnitTemplate Template;
        public int Cost;
        public BuildingUpgrades Upgrades = new BuildingUpgrades();
        public UiBuildingUnitInfoController UnitInfoTemplate = null;

        public Vector2Int[] GetRequiredPositions(Vector2Int origin)
        {
            var returnTiles = new List<Vector2Int> {origin};
            for (var i = 0; i < RequiredTiles.Length; i++)
            {
                var pos = RequiredTiles[i] + origin;
                if (!returnTiles.Contains(pos))
                {
                    returnTiles.Add(pos);
                }
            }

            return returnTiles.ToArray();
        }

        public Vector2Int[] GetBlockingPositions(Vector2Int origin)
        {
            var returnTiles = new List<Vector2Int>();
            for (var i = 0; i < BlockingTiles.Length; i++)
            {
                var pos = BlockingTiles[i] + origin;
                if (!returnTiles.Contains(pos))
                {
                    returnTiles.Add(pos);
                }
            }

            return returnTiles.ToArray();
        }
    }
}