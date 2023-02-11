using System;
using Assets.Ancible_Tools.Scripts.Traits;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    [Serializable]
    public class SkillLevel
    {
        public Trait[] ApplyOnLevel;
        public float Bonus;
    }
}