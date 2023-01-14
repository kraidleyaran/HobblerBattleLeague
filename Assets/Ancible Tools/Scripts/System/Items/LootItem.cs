using System;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class LootItem
    {
        public WorldItem Item = null;
        public IntNumberRange Stack = new IntNumberRange();
        [Range(0f,1f)] public float ChanceToDrop;
    }
}