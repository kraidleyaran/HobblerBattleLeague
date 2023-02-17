using System;
using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [Serializable]
    public class StatusIcon
    {
        public Sprite Icon = null;
        public Color Color = Color.white;
        public WellbeingStatType Type = WellbeingStatType.Boredom;
    }
}