using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Factories
{
    public class TraitFactory : MonoBehaviour
    {
        private static TraitFactory _instance = null;

        [SerializeField] private string[] _spriteFolders = new string[0];

        private Dictionary<string, SpriteTrait> _sprites = new Dictionary<string, SpriteTrait>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            var spriteTraits = new List<SpriteTrait>();
            foreach (var folder in _spriteFolders)
            {
                spriteTraits.AddRange(UnityEngine.Resources.LoadAll<SpriteTrait>(folder));
            }

            _sprites = spriteTraits.ToDictionary(s => s.name, s => s);
            Debug.Log($"Loaded {_sprites.Count} Sprite Traits");
        }

        public static SpriteTrait GetSpriteByName(string spriteName)
        {
            if (_instance._sprites.TryGetValue(spriteName, out var sprite))
            {
                return sprite;
            }

            return null;
        }
    }
}