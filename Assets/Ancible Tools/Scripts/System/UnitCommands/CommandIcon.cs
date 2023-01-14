using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands
{
    [Serializable]
    public class CommandIcon
    {
        public Sprite Sprite = null;
        public Vector2 Offset = Vector2.zero;
        public Color ColorMask = Color.white;
        public bool FlipX;
        public bool FlipY;
    }
}