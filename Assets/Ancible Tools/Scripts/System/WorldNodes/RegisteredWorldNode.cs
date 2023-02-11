using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Pathing;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.WorldNodes
{
    [Serializable]
    public class RegisteredWorldNode
    {
        public GameObject Unit;
        public MapTile Tile;
        public WorldNodeType Type;
        public int Priority;

        public RegisteredWorldNode(GameObject unit, MapTile tile, WorldNodeType type)
        {
            Unit = unit;
            Tile = tile;
            Type = type;
        }

        public void Destroy()
        {
            Unit = null;
            Tile = null;
        }
    }
}