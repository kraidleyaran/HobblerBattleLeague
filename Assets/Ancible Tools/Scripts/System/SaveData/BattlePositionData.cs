using System;
using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class BattlePositionData : IDisposable
    {
        public string Id = string.Empty;
        public Vector2IntData Position = Vector2IntData.Zero;

        public void Dispose()
        {
            Position = Vector2IntData.Zero;
            Id = null;
        }
    }
}