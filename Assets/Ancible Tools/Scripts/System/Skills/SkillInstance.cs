using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Skills
{
    public class SkillInstance : WorldInstance<WorldSkill>
    {
        public int Level { get; private set; }
        public int Experience { get; private set; }

        public SkillInstance(WorldSkill skill)
        {
            Instance = skill;
            Level = 0;
            Experience = 0;
        }

        public SkillInstance(WorldSkill skill, SkillData data)
        {
            Instance = skill;
            Level = data.Level;
            Experience = data.Experience;
        }

        public int GainExperience(int amount)
        {
            if (Level < Instance.Levels.Length - 1)
            {
                var experience = Experience += amount;
                var levelsGained = 0;
                var requiredExperience = Instance.CalculateExperienceForLevel(Level + 1);
                while (experience >= requiredExperience)
                {
                    experience -= requiredExperience;
                    if (Level + levelsGained + 1 < Instance.Levels.Length)
                    {
                        levelsGained++;
                    }
                    else
                    {
                        experience = -1;
                        requiredExperience = 0;
                    }
                }

                if (experience < 0)
                {
                    experience = 0;
                }

                Experience = experience;
                return levelsGained;
            }
            return 0;

        }

        public void ApplyLevels(int levels, GameObject owner)
        {
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            var levelCount = 0;
            while (levelCount < levels)
            {
                if (Level + 1 < Instance.Levels.Length)
                {
                    Level++;
                    var traits = Instance.Levels[Level - 1].ApplyOnLevel;
                    for (var i = 0; i < traits.Length; i++)
                    {
                        addTraitToUnitMsg.Trait = traits[i];
                        owner.SendMessageTo(addTraitToUnitMsg, owner);
                    }
                }

                levelCount++;
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        public void SetLevel(int level)
        {
            Level = level;
        }

        public void SetExperience(int experience)
        {
            Experience = experience;
        }

        public void SetFromData(SkillData data, GameObject owner = null)
        {
            Experience = data.Experience;
            if (owner)
            {
                ApplyLevels(data.Level, owner);
            }
            else
            {
                Level = data.Level;
            }

        }
    }
}