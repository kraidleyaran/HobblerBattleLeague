﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Loot Table", menuName = "Ancible Tools/Items/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [SerializeField] private LootItem[] _items = new LootItem[0];
        [SerializeField] private IntNumberRange _itemRolls = new IntNumberRange();
        [SerializeField] [Range(0f, 1f)] private float _rollBonus = 0f;

        public ItemStack[] GenerateLoot()
        {
            var returnItems = new Dictionary<WorldItem, int>();
            var items = _itemRolls.Roll();
            for (var i = 0; i < items; i++)
            {
                var itemRoll = (Random.Range(0f, 1f) + _rollBonus);
                var availableItems = _items.Where(it => it.ChanceToDrop >= itemRoll).ToArray();
                if (availableItems.Length > 0)
                {
                    var item = availableItems.Length > 1 ? availableItems[Random.Range(0, availableItems.Length)] : availableItems[0];
                    if (!returnItems.ContainsKey(item.Item))
                    {
                        returnItems.Add(item.Item, 0);
                    }
                    returnItems[item.Item] += item.Stack.Roll();
                }
            }

            return returnItems.Select(kv => new ItemStack {Item = kv.Key, Stack = kv.Value}).ToArray();
        }


    }
}