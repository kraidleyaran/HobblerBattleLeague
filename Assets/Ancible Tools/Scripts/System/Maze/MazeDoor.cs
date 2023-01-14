using ProceduralToolkit;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Maze
{
    public class MazeDoor
    {
        public Vector2Int Position;
        public Directions Rotation;
        public Vector2Int[] Walls;


        public MazeDoor(Vector2Int position, Directions direction, Vector2Int[] walls)
        {
            Position = position;
            Rotation = direction;
            Walls = walls;
        }
    }
}