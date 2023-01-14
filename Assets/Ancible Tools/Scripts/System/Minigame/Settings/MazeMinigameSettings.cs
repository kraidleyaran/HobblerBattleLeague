using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Maze;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    [CreateAssetMenu(fileName = "Maze Minigame Settings", menuName = "Ancible Tools/Minigame Settings/Maze Minigame Settings")]
    public class MazeMinigameSettings : MinigameSettings
    {
        public override MinigameType Type => MinigameType.Maze;
        public Vector2Int Size = Vector2Int.zero;
        public int WallSize = 4;
        public int RoomSize = 5;
        public int EdgeBuffer = 4;
        [Range(0f, 1f)] public float DoorPerecent = 1f;
        public UnitTemplate[] MonsterTemplates = new UnitTemplate[0];
        public IntNumberRange MonstersPerRoom = new IntNumberRange();
        public UnitTemplate[] ChestTemplates = new UnitTemplate[0];
        public LootTable[] ChestLootTables = new LootTable[0];
        public int MaximumChestsPerRoom = 1;
        [Range(0f,1f)] public float ChestSpawnPercent = 1f;
    }
}