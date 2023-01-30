using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Factories
{
    public class WorldAbilityFactory : MonoBehaviour
    {
        private static WorldAbilityFactory _instance = null;

        [SerializeField] private string[] _abilityFolders = new string[0];

        private Dictionary<string, WorldAbility> _abilities = new Dictionary<string, WorldAbility>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            var abilities = new List<WorldAbility>();
            foreach (var folder in _abilityFolders)
            {
                abilities.AddRange(UnityEngine.Resources.LoadAll<WorldAbility>(folder));
            }

            _abilities = abilities.ToDictionary(a => a.name, a => a);
        }

        public static WorldAbility GetAbilityByName(string abilityName)
        {
            if (_instance._abilities.TryGetValue(abilityName, out var ability))
            {
                return ability;
            }

            return null;
        }
    }
}