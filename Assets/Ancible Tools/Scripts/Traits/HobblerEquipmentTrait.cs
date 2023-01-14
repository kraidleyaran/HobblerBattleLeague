using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Equipment Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler Equipment")]
    public class HobblerEquipmentTrait : Trait
    {
        [SerializeField] private WeaponItem _startingWeapon = null;

        private Dictionary<int, EquippableInstance> _armorSlots = new Dictionary<int, EquippableInstance>
        {
            {0, null },
            {1, null },
            {2, null }
        };        

        private Dictionary<int, EquippableInstance> _trinketSlots = new Dictionary<int, EquippableInstance>
        {
            {0, null},
            {1, null}
        };

        private EquippableInstance _weaponSlot = null;

        private List<int> _autoArmorIndexes = new List<int>{0,1,2};
        private List<int> _autoTrinketIndexes = new List<int>{0,1};

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (_startingWeapon)
            {
                _controller.StartCoroutine(StaticMethods.WaitForFrames(1, () =>
                {
                    if (_weaponSlot == null)
                    {
                        _weaponSlot = new EquippableInstance(_startingWeapon, _controller.transform.parent.gameObject);
                    }
                }));
                
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<EquipItemToSlotMessage>(EquipItemToSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnequipItemFromSlotMessage>(UnequipItemFromSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryHobblerEquipmentMessage>(QueryHobblerEquipment, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetEquipmentMessage>(SetEquipment, _instanceId);
        }

        private void EquipItemToSlot(EquipItemToSlotMessage msg)
        {
            switch (msg.Item.Slot)
            {
                case EquipSlot.Armor:
                    var armorIndex = msg.Index;
                    if (armorIndex < 0)
                    {
                        armorIndex = _autoArmorIndexes[0];
                    }
                    if (_armorSlots.ContainsKey(armorIndex))
                    {
                        if (_armorSlots[armorIndex] != null)
                        {
                            WorldStashController.AddItem(_armorSlots[armorIndex].Instance);
                            _armorSlots[msg.Index].Destroy();
                        }
                        _armorSlots[armorIndex] = new EquippableInstance(msg.Item, _controller.transform.parent.gameObject);
                        _autoArmorIndexes.Remove(armorIndex);
                        _autoArmorIndexes.Add(armorIndex);
                    }
                    break;
                case EquipSlot.Weapon:
                    if (_weaponSlot != null)
                    {
                        WorldStashController.AddItem(_weaponSlot.Instance);
                        _weaponSlot.Destroy();
                    }
                    _weaponSlot = new EquippableInstance(msg.Item, _controller.transform.parent.gameObject);
                    break;
                case EquipSlot.Trinket:
                    var trinketIndex = msg.Index;
                    if (trinketIndex < 0)
                    {
                        trinketIndex = _autoTrinketIndexes[0];
                    }
                    if (_trinketSlots.ContainsKey(trinketIndex))
                    {
                        if (_trinketSlots[trinketIndex] != null)
                        {
                            WorldStashController.AddItem(_trinketSlots[trinketIndex].Instance);
                            _trinketSlots[trinketIndex].Destroy();
                        }
                        _trinketSlots[msg.Index] = new EquippableInstance(msg.Item, _controller.transform.parent.gameObject);
                        _autoTrinketIndexes.Remove(trinketIndex);
                        _autoTrinketIndexes.Add(trinketIndex);
                    }
                    break;
            }
            _controller.gameObject.SendMessageTo(EquipmentUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void UnequipItemFromSlot(UnequipItemFromSlotMessage msg)
        {
            switch (msg.Slot)
            {
                case EquipSlot.Armor:
                    if (_armorSlots.ContainsKey(msg.Index))
                    {
                        if (_armorSlots[msg.Index] != null)
                        {
                            if (msg.ReturnToStash)
                            {
                                WorldStashController.AddItem(_armorSlots[msg.Index].Instance);
                            }
                            _armorSlots[msg.Index].Destroy();
                            _armorSlots[msg.Index] = null;
                        }
                    }

                    break;
                case EquipSlot.Weapon:
                    if (_weaponSlot != null)
                    {
                        if (msg.ReturnToStash)
                        {
                            WorldStashController.AddItem(_weaponSlot.Instance);
                        }
                        
                        _weaponSlot.Destroy();
                        _weaponSlot = null;
                    }
                    break;
                case EquipSlot.Trinket:
                    if (_trinketSlots.ContainsKey(msg.Index))
                    {
                        if (_trinketSlots[msg.Index] != null)
                        {
                            if (msg.ReturnToStash)
                            {
                                WorldStashController.AddItem(_trinketSlots[msg.Index].Instance);
                            }
                            _trinketSlots[msg.Index].Destroy();
                            _trinketSlots[msg.Index] = null;
                        }
                    }
                    break;
            }
            _controller.gameObject.SendMessageTo(EquipmentUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void QueryHobblerEquipment(QueryHobblerEquipmentMessage msg)
        {
            msg.DoAfter.Invoke(_armorSlots.Values.ToArray(), _trinketSlots.Values.ToArray(), _weaponSlot);
        }

        private void SetEquipment(SetEquipmentMessage msg)
        {
            var equipItemToSlotMsg = MessageFactory.GenerateEquipItemToSlotMsg();
            for (var i = 0; i < msg.Items.Length; i++)
            {
                equipItemToSlotMsg.Item = msg.Items[i];
                equipItemToSlotMsg.Index = -1;
                _controller.gameObject.SendMessageTo(equipItemToSlotMsg, _controller.transform.parent.gameObject);
            }
            MessageFactory.CacheMessage(equipItemToSlotMsg);
        }
    }
}