using System;
using System.Numerics;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class WindowData
    {
        public string Window;
        public string Id;
        public Vector2IntData Position;
    }
}