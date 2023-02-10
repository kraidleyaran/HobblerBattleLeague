using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Items.Crafting;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Factories
{
    public class WorldItemFactory : MonoBehaviour
    {
        private static WorldItemFactory _instance = null;

        [SerializeField] private string[] _itemFolders = new string[0];
        [SerializeField] private string[] _recipeFolders = new string[0];

        private Dictionary<string, WorldItem> _items = new Dictionary<string, WorldItem>();
        private Dictionary<string, CraftingRecipe> _recipes = new Dictionary<string, CraftingRecipe>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            var items = new List<WorldItem>();
            foreach (var folder in _itemFolders)
            {
                items.AddRange(UnityEngine.Resources.LoadAll<WorldItem>(folder));
            }

            _items = items.ToDictionary(i => i.name, i => i);
            Debug.Log($"Loaded {_items.Count} World Items");

            var recipes = new List<CraftingRecipe>();
            foreach (var folder in _recipeFolders)
            {
                recipes.AddRange(UnityEngine.Resources.LoadAll<CraftingRecipe>(folder));
            }

            _recipes = recipes.ToDictionary(r => r.name, r => r);
        }

        public static WorldItem GetItemByName(string itemName)
        {
            if (_instance._items.TryGetValue(itemName, out var worldItem))
            {
                return worldItem;
            }

            return null;
        }

        public static CraftingRecipe GetRecipeByName(string recipeName)
        {
            if (_instance._recipes.TryGetValue(recipeName, out var recipe))
            {
                return recipe;
            }

            return null;
        }

        void OnDestroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
        }
    }
}