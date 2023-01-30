using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class SkillData
    {
        public string Skill;
        public int Experience;
        public int Level;
        public int Priority;

        public void Dispose()
        {
            Skill = null;
            Experience = 0;
            Level = 0;
            Priority = 0;
        }
    }
}