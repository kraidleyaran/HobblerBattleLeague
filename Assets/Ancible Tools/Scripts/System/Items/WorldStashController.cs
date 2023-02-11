using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class WorldStashController : MonoBehaviour
    {
        public static int Gold { get; private set; }

        private static WorldStashController _instance = null;

        [SerializeField] private ItemStack[] _startingItems = new ItemStack[0];
        [SerializeField] private int _startingGold = 0;

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

            Gold = _startingGold;
            StartCoroutine(StaticMethods.WaitForFrames(5, () =>
            {
                gameObject.SendMessage(GoldUpdatedMessage.INSTANCE);
            }));
            SubscribeToMessages();
        }

        public static void AddItem(WorldItem item, int stack = 1)
        {
            if (item.Type == WorldItemType.Gold)
            {
                AddGold(stack);
            }
            else if (item.Type == WorldItemType.Instant)
            {
                //TODO: One day use this?
            }
            else
            {
                var existingStacks = _instance._items.Where(s => s.Item == item && s.Stack < item.MaxStack).OrderByDescending(s => s.Stack).ToArray();
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

        public static void SetStashFromData(ItemStackData[] items, int gold)
        {
            Clear();
            SetGold(gold);
            foreach (var item in items)
            {
                var worldItem = WorldItemFactory.GetItemByName(item.Item);
                if (worldItem)
                {
                    AddItem(worldItem, item.Stack);
                }
            }
            _instance.gameObject.SendMessage(StashUpdatedMessage.INSTANCE);
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



        public static ItemStackData[] GetStashData()
        {
            return _instance._items.Select(i => i.ToData()).ToArray();
        }

        public static bool HasItem(WorldItem item, int stack = 1)
        {
            if (stack > 1)
            {
                var items = _instance._items.FindAll(i => i.Item == item);
                var totalStack = items.Sum(i => i.Stack);
                return totalStack >= stack;
            }
            else
            {
                return _instance._items.Exists(i => i.Item == item);
            }
            
        }

        public static void Clear()
        {
            Gold = 0;
            var items = _instance._items.ToArray();
            foreach (var item in items)
            {
                item.Destroy();
            }
            _instance._items = new List<ItemStack>();
        }

        private static void SetGold(int amount)
        {
            Gold = amount;
            _instance.gameObject.SendMessage(GoldUpdatedMessage.INSTANCE);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ClearWorldMessage>(ClearWorld);
        }

        private void ClearWorld(ClearWorldMessage msg)
        {
            Clear();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}