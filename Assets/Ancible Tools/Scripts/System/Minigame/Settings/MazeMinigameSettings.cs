using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.Maze;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using Assets.Resources.Ancible_Tools.Scripts.System.Templates;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    [CreateAssetMenu(fileName = "Maze Minigame Settings", menuName = "Ancible Tools/Minigame Settings/Maze Minigame Settings")]
    public class MazeMinigameSettings : MinigameSettings
    {
        public override MinigameType Type => MinigameType.Maze;
        public string DisplayName;
        public Sprite Icon;
        public Vector2Int Size = Vector2Int.zero;
        public Tileset Ground = null;
        public int[] GroundTiles;
        public Tileset Terrain = null;
        public int WallTileId;
        public int WallSize = 4;
        public int RoomSize = 5;
        public bool PaintGroundForWall = false;
        public int EdgeBuffer = 4;
        public MinigameUnitSpawnController VerticalDoorSpawn;
        public CustomTileData[] VerticalDoorOverTiles = new CustomTileData[0];
        public MinigameUnitSpawnController HorizontalDoorSpawn;
        public CustomTileData[] HorizontalDoorOverTiles = new CustomTileData[0];
        public UnitTemplate UpEndTemplate;
        public UnitTemplate DownEndTemplate;
        public UnitTemplate LeftEndTemplate;
        public UnitTemplate RightEndTemplate;
        [Range(0f, 1f)] public float DoorPerecent = 1f;
        public UnitTemplate[] MonsterTemplates = new UnitTemplate[0];
        public IntNumberRange MonstersPerRoom = new IntNumberRange();
        public UnitTemplate[] ChestTemplates = new UnitTemplate[0];
        public LootTable[] ChestLootTables = new LootTable[0];
        public int MaximumChestsPerRoom = 1;
        [Range(0f,1f)] public float ChestSpawnPercent = 1f;
    }
}