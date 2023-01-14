using System.Collections.Generic;
using System.Linq;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class WorldStashController : MonoBehaviour
    {
        public static int Gold { get; private set; }

        private static WorldStashController _instance = null;

        [SerializeField] private ItemStack[] _startingItems = new ItemStack[0];

        private List<ItemStack> _items = new List<ItemStack>();

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            for (var i = 0; i < _startingItems.Length; i++)
            {
                AddItem(_startingItems[i].Item, _startingItems[i].Stack);
            }
        }

        public static void AddItem(WorldItem item, int stack = 1)
        {
            if (item.Type == WorldItemType.Gold)
            {
                AddGold(stack);
            }
            else
            {
                var existingStacks = _instance._items.Where(s => s.Stack < item.MaxStack).OrderByDescending(s => s.Stack).ToArray();
                if (existingStacks.Length > 0)
                {
                    var remainingStack = stack;
                    for (var i = 0; i < existingStacks.Length && remainingStack > 0; i++)
                    {
                        var currentStack = existingStacks[i];
                        var addStack = item.MaxStack - currentStack.Stack;
                        if (addStack > remainingStack)
                        {
                            addStack = remainingStack;
                            remainingStack = 0;
                        }
                        else
                        {
                            remainingStack -= addStack;

                        }
                        currentStack.Stack += addStack;
                    }

                    if (remainingStack > 0)
                    {
                        AddItem(item, remainingStack);
                    }
                }
                else
                {
                    if (item.MaxStack <= stack)
                    {
                        var itemStack = new ItemStack { Item = item, Stack = item.MaxStack };
                        _instance._items.Add(itemStack);
                        var remainingStack = stack - item.MaxStack;
                        if (remainingStack > 0)
                        {
                            AddItem(item, remainingStack);
                        }
                    }
                    else
                    {
                        var itemStack = new ItemStack { Item = item, Stack = stack };
                        _instance._items.Add(itemStack);
                    }
                }
                _instance.gameObject.SendMessage(StashUpdatedMessage.INSTANCE);
            }

        }

        public static WorldItem RemoveItem(WorldItem item, int stack = 1)
        {
            var remainingStack = stack;
            var existingStacks = _instance._items.Where(s => s.Item == item).OrderByDescending(s => s.Stack).ToArray();
            for (var i = 0; i < existingStacks.Length && remainingStack > 0; i++)
            {
                if (existingStacks[i].Stack < remainingStack)
                {
                    remainingStack -= existingStacks[i].Stack;
                    existingStacks[i].Destroy();
                    _instance._items.Remove(existingStacks[i]);
                }
                else
                {
                    existingStacks[i].Stack -= remainingStack;
                    remainingStack = 0;
                    if (existingStacks[i].Stack <= 0)
                    {
                        existingStacks[i].Destroy();
                        _instance._items.Remove(existingStacks[i]);
                    }
                }
                _instance.gameObject.SendMessage(StashUpdatedMessage.INSTANCE);
            }
            return remainingStack < stack ? item : null;
        }

        public static ItemStack[] GetItems()
        {
            return _instance._items.ToArray();
        }

        public static void AddGold(int amount)
        {
            Gold += amount;
            _instance.gameObject.SendMessage(GoldUpdatedMessage.INSTANCE);
        }

        public static void RemoveGold(int amount)
        {
            Gold = Mathf.Max(0, Gold - amount);
            _instance.gameObject.SendMessage(GoldUpdatedMessage.INSTANCE);
        }
    }
}