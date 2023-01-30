using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Factories
{
    public class WorldSkillFactory : MonoBehaviour
    {
        private static WorldSkillFactory _instance = null;

        [SerializeField] private string[] _skillsFolders = new string[0];

        private Dictionary<string, WorldSkill> _skills = new Dictionary<string, WorldSkill>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            var skills = new List<WorldSkill>();
            foreach (var folder in _skillsFolders)
            {
                skills.AddRange(UnityEngine.Resources.LoadAll<WorldSkill>(folder));
            }

            _skills = skills.ToDictionary(s => s.name, s => s);
        }

        public static WorldSkill GetSkillByName(string skillName)
        {
            if (_instance._skills.TryGetValue(skillName, out var skill))
            {
                return skill;
            }

            return null;
        }
    }
}