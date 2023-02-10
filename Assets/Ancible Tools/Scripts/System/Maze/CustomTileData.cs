using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Maze
{
    [Serializable]
    public class CustomTileData
    {
        public int TileData;
        public bool Brush;
        public Vector2Int Position;

        public CustomTileData FromRelativePosition(Vector2Int pos)
        {
            return new CustomTileData {TileData = TileData, Brush = Brush, Position = Position + pos};
        }
    }
}