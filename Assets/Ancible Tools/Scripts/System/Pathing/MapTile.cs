using System;
using RogueSharp;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Pathing
{
    public delegate void OnTileEntry(GameObject obj);

    [Serializable]
    public class MapTile
    {
        public Vector2Int Position;
        public ICell Cell;
        public Vector2 World;
        public GameObject Block;
        public event OnTileEntry OnObjectEnteringTile = null;

        public void ApplyEvent(GameObject obj)
        {
            OnObjectEnteringTile?.Invoke(obj);
        }

        public void Destroy()
        {
            Position = Vector2Int.zero;
            Cell = null;
            World = Vector2.zero;
            
        }
    }
}