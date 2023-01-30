using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public struct Vector2IntData
    {
        public static Vector2IntData Zero = new Vector2IntData {X = 0, Y = 0};

        public int X;
        public int Y;

        public Vector2Int ToVector()
        {
            return new Vector2Int(X,Y);
        }
    }
}