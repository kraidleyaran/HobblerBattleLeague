using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Maze
{
    [SerializeField]
    public class MazeRoom
    {
        public Vector2Int RoomId;
        public Vector2Int Min;
        public Vector2Int Max;
        public Vector2Int[] PathableTiles = new Vector2Int[0];
        public List<Vector2Int> SpawnableTiles = new List<Vector2Int>();
        public Vector2Int[] DeadEnds = new Vector2Int[0];
        public int Depth;
        
    }
}