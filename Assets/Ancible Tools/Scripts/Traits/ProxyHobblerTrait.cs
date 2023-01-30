using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Proxy Hobbler Trait", menuName = "Ancible Tools/Traits/Minigame/Player/Proxy Hobbler")]
    public class ProxyHobblerTrait : Trait
    {
        private GameObject _proxyUnit = null;

        private int _experience = 0;
        private int _gold = 0;
        private IntNumberRange _monsterKills = IntNumberRange.One;
        private IntNumberRange _chestsFound = IntNumberRange.One;
        private Dictionary<WorldItem, ItemStack> _items = new Dictionary<WorldItem, ItemStack>();
        private List<ItemStack> _debugStacks = new List<ItemStack>();
        private EquippableInstance[] _equippedItems = new EquippableInstance[0];

        private List<GameObject> _claimedKills = new List<GameObject>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            //_controller.gameObject.Subscribe<EndMinigameMessage>(EndMinigame);
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddExperienceMessage>(AddExperience, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetProxyHobblerMessage>(SetProxyHobler, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<AddItemMessage>(AddItem, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetMinigameEquipmentMessage>(SetMinigameEquipment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobblerEquipmentMessage>(QueryHobblerEquipment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryProxyRewardsMessage>(QueryProxyRewards, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ClaimKillMessage>(ClaimKill, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ClaimChestMessage>(ClaimChest, _instanceId);
        }

        private void AddExperience(AddExperienceMessage msg)
        {
            _experience += msg.Amount;
        }

        private void SetProxyHobler(SetProxyHobblerMessage msg)
        {
            _proxyUnit = msg.Hobbler;
            _chestsFound = new IntNumberRange{Minimum = 0, Maximum = msg.MaxChests};
            _monsterKills = new IntNumberRange {Minimum = 0, Maximum = msg.MaxMonsters};
        }

        private void AddItem(AddItemMessage msg)
        {
            if (msg.Item.Type == WorldItemType.Gold)
            {
                _gold += msg.Stack;
                UiAlertManager.ShowAlert($"+{msg.Stack} Gold", IconFactoryController.Gold);
            }
            else
            {
                if (!_items.TryGetValue(msg.Item, out var itemStack))
                {
                    itemStack = new ItemStack { Item = msg.Item, Stack = 0 };
                    _items.Add(msg.Item, itemStack);
                    _debugStacks.Add(itemStack);
                }

                itemStack.Stack += msg.Stack;
                UiAlertManager.ShowAlert($"+{msg.Stack} {itemStack.Item.DisplayName}", itemStack.Item.Icon);
            }

        }

        private void QueryHobblerEquipment(QueryHobblerEquipmentMessage msg)
        {
            var armor = _equippedItems.Where(e => e != null && e.Instance.Slot == EquipSlot.Armor).ToArray();
            var trinkets = _equippedItems.Where(e => e != null && e.Instance.Slot == EquipSlot.Trinket).ToArray();
            var weapon = _equippedItems.FirstOrDefault(e => e.Instance.Slot == EquipSlot.Weapon);
            msg.DoAfter.Invoke(armor, trinkets, weapon);
        }

        private void SetMinigameEquipment(SetMinigameEquipmentMessage msg)
        {
            _equippedItems = msg.Equipment.ToArray();
        }

        private void QueryProxyRewards(QueryProxyRewardsMessage msg)
        {
            msg.DoAfter.Invoke(_experience, _gold, _monsterKills, _chestsFound, _items.Values.Select(i => i.Clone()).ToArray());
        }

        private void ClaimKill(ClaimKillMessage msg)
        {
            if (!_claimedKills.Contains(msg.Kill) && _monsterKills.Minimum < _monsterKills.Maximum)
            {
                _monsterKills.Minimum++;
            }
        }

        private void ClaimChest(ClaimChestMessage msg)
        {
            if (_chestsFound.Minimum < _chestsFound.Maximum)
            {
                _chestsFound.Minimum++;
            }
        }

        public override void Destroy()
        {
            _experience = 0;
            if (_items.Count > 0)
            {
                var itemStacks = _items.Values.ToArray();
                for (var i = 0; i < itemStacks.Length; i++)
                {
                    itemStacks[i].Destroy();
                }
                _items.Clear();
            }
            _items = null;
            base.Destroy();
        }
    }
}