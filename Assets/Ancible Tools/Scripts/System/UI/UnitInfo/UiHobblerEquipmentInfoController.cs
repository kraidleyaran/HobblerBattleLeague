using System;
using Assets.Ancible_Tools.Scripts.Traits;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DetailedInfo;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DragDrop;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiHobblerEquipmentInfoController : MonoBehaviour
    {
        private const string FILTER = "UI_HOBBLER_EQUIPMENT_INFO";

        [SerializeField] private Image _spriteIcon = null;
        [SerializeField] private UiExperienceBarController _experienceBarController = null;
        [Header("Equipment References")]
        [SerializeField] private UiEquippedItemController[] _armorControllers = new UiEquippedItemController[0];
        [SerializeField] private UiEquippedItemController[] _trinketControllers = new UiEquippedItemController[0];
        [SerializeField] private UiEquippedItemController _weaponController = null;
        [Space]
        [Header("Stat References")]
        [SerializeField] private UiCombatStatController _healthController;
        [SerializeField] private UiCombatStatController _spiritController;
        [SerializeField] private UiCombatStatController _strengthController;
        [SerializeField] private UiCombatStatController _agilityController;
        [SerializeField] private UiCombatStatController _defenseController;
        [SerializeField] private UiCombatStatController _manaController;
        [SerializeField] private UiCombatStatController _magicController;
        [SerializeField] private UiCombatStatController _faithController;

        private GameObject _unit = null;
        private UiEquippedItemController _hoveredItem = null;
        private int _dragAndDropIndex = -1;

        public void Setup(GameObject unit)
        {
            _unit = unit;
            RefreshInfo();
            _experienceBarController.Setup(unit);
            _unit.SubscribeWithFilter<EquipmentUpdatedMessage>(EquipmentUpdated, FILTER);
            SubscribeToMessages();
        }

        public void RefreshInfo()
        {
            var queryHobblerEquipmentMsg = MessageFactory.GenerateQueryHobblerEquipmentMsg();
            queryHobblerEquipmentMsg.DoAfter = RefreshEquipment;
            gameObject.SendMessageTo(queryHobblerEquipmentMsg, _unit);
            MessageFactory.CacheMessage(queryHobblerEquipmentMsg);

            var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
            queryCombatStatsMsg.DoAfter = RefreshCombatStats;
            gameObject.SendMessageTo(queryCombatStatsMsg, _unit);
            MessageFactory.CacheMessage(queryCombatStatsMsg);

            var querySpriteMsg = MessageFactory.GenerateQuerySpriteMsg();
            querySpriteMsg.DoAfter = RefreshSprite;
            gameObject.SendMessageTo(querySpriteMsg, _unit);
            MessageFactory.CacheMessage(querySpriteMsg);
        }

        private void OnDropFailure(UiEquippedItemController controller)
        {
            if (controller.Item && (!_hoveredItem || _hoveredItem != controller))
            {
                var unequipItemFromSlotMsg = MessageFactory.GenerateUnequipItemFromSlotMsg();
                unequipItemFromSlotMsg.Index = _dragAndDropIndex;
                unequipItemFromSlotMsg.ReturnToStash = true;
                unequipItemFromSlotMsg.Slot = controller.Item.Slot;
                gameObject.SendMessageTo(unequipItemFromSlotMsg, _unit);
                MessageFactory.CacheMessage(unequipItemFromSlotMsg);
                _dragAndDropIndex = -1;
            }
        }



        private void RefreshEquipment(EquippableInstance[] armor, EquippableInstance[] trinkets, EquippableInstance weapon)
        {
            for (var i = 0; i < _armorControllers.Length; i++)
            {
                _armorControllers[i].Setup(armor.Length > i ? armor[i]?.Instance : null, gameObject);
            }

            for (var i = 0; i < _trinketControllers.Length; i++)
            {
                _trinketControllers[i].Setup(trinkets.Length > i ? trinkets[i]?.Instance : null, gameObject);
            }
            _weaponController.Setup(weapon?.Instance, gameObject);
        }

        private void RefreshCombatStats(CombatStats baseStats, CombatStats bonus, GeneticCombatStats genetics)
        {
            var stats = baseStats + genetics;
            _healthController.Setup(stats.Health, bonus.Health);
            _spiritController.Setup(stats.Spirit, bonus.Spirit);
            _strengthController.Setup(stats.Strength, bonus.Strength);
            _agilityController.Setup(stats.Agility, bonus.Agility);
            _defenseController.Setup(stats.Defense, bonus.Defense);
            _manaController.Setup(stats.Mana, bonus.Mana);
            _magicController.Setup(stats.Magic, bonus.Magic);
            _faithController.Setup(stats.Faith, bonus.Faith);
        }

        private void RefreshSprite(SpriteTrait sprite)
        {
            _spriteIcon.sprite = sprite.Sprite;
        }

        private void EquipmentUpdated(EquipmentUpdatedMessage msg)
        {
            RefreshInfo();
        }

        private void ReceiveDragDropItem(ReceiveDragDropItemMessage msg)
        {
            if (msg.Owner != gameObject)
            {
                if (msg.Item.Type == WorldItemType.Equippable && msg.Item is EquippableItem equippable)
                {
                    var index = -1;
                    if (_hoveredItem && equippable.Slot == _hoveredItem.Slot)
                    {
                        switch (_hoveredItem.Slot)
                        {
                            case EquipSlot.Armor:
                                index = Array.IndexOf(_armorControllers, _hoveredItem);
                                break;
                            case EquipSlot.Trinket:
                                index = Array.IndexOf(_trinketControllers, _hoveredItem);
                                break;
                        }
                    }

                    var equipItemToSlotMsg = MessageFactory.GenerateEquipItemToSlotMsg();
                    equipItemToSlotMsg.Item = equippable;
                    equipItemToSlotMsg.Index = index;
                    gameObject.SendMessageTo(equipItemToSlotMsg, _unit);
                    MessageFactory.CacheMessage(equipItemToSlotMsg);

                    var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                    removeItemMsg.Item = equippable;
                    removeItemMsg.Stack = 1;
                    gameObject.SendMessageTo(removeItemMsg, msg.Owner);
                    MessageFactory.CacheMessage(removeItemMsg);
                    msg.DoAfter.Invoke();
                }
                
            }


        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
            gameObject.SubscribeWithFilter<ReceiveDragDropItemMessage>(ReceiveDragDropItem, FILTER);
            gameObject.SubscribeWithFilter<SetHoveredEquippedItemControllerMessage>(SetHoveredEquippedItemController, FILTER);
            gameObject.SubscribeWithFilter<RemoveHoveredEquippedItemControllerMessage>(RemoveHoveredEquippedItemController, FILTER);
            gameObject.SubscribeWithFilter<RemoveItemMessage>(RemoveItem, FILTER);
        }

        private void SetHoveredEquippedItemController(SetHoveredEquippedItemControllerMessage msg)
        {
            _hoveredItem = msg.Controller;
        }

        private void RemoveHoveredEquippedItemController(RemoveHoveredEquippedItemControllerMessage msg)
        {
            if (_hoveredItem && _hoveredItem == msg.Controller)
            {
                _hoveredItem = null;
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (gameObject.activeSelf)
            {
                if (_hoveredItem && _hoveredItem.Item)
                {
                    if (msg.Previous.LeftClick && msg.Current.LeftClick && !UiDragDropManager.Active)
                    {
                        var hovered = _hoveredItem;
                        UiDragDropManager.SetDragDropItem(gameObject, _hoveredItem.Item, 1, () => { OnDropFailure(hovered); });
                        switch (_hoveredItem.Slot)
                        {
                            case EquipSlot.Armor:
                                _dragAndDropIndex = Array.IndexOf(_armorControllers, _hoveredItem);
                                break;
                            case EquipSlot.Trinket:
                                _dragAndDropIndex = Array.IndexOf(_trinketControllers, _hoveredItem);
                                break;
                        }
                    }
                }
            }

        }

        private void RemoveItem(RemoveItemMessage msg)
        {
            if (msg.Item.Type == WorldItemType.Equippable && msg.Item is EquippableItem equippable)
            {
                var index = _dragAndDropIndex;
                _dragAndDropIndex = -1;
                var unequipItemFromSlotMsg = MessageFactory.GenerateUnequipItemFromSlotMsg();
                unequipItemFromSlotMsg.ReturnToStash = false;
                switch (equippable.Slot)
                {
                    case EquipSlot.Weapon:
                        if (_weaponController.Item && _weaponController.Item == equippable)
                        {
                            unequipItemFromSlotMsg.Slot = EquipSlot.Weapon;
                            gameObject.SendMessageTo(unequipItemFromSlotMsg, _unit);
                        }
                        break;
                    case EquipSlot.Armor:
                        if (index >= 0 && _armorControllers.Length > index && _armorControllers[index].Item && _armorControllers[index].Item == equippable)
                        {
                            unequipItemFromSlotMsg.Index = index;
                            unequipItemFromSlotMsg.Slot = EquipSlot.Armor;
                            gameObject.SendMessageTo(unequipItemFromSlotMsg, _unit);
                        }
                        break;
                    case EquipSlot.Trinket:
                        if (index >= 0 && _trinketControllers.Length > index && _trinketControllers[index].Item &&
                            _trinketControllers[index].Item == equippable)
                        {
                            unequipItemFromSlotMsg.Index = index;
                            unequipItemFromSlotMsg.Slot = EquipSlot.Trinket;
                            gameObject.SendMessageTo(unequipItemFromSlotMsg, _unit);
                        }
                        break;
                }
                MessageFactory.CacheMessage(unequipItemFromSlotMsg);
            }
        }

        public void Destroy()
        {
            if (_unit)
            {
                _unit.UnsubscribeFromAllMessagesWithFilter(FILTER);
            }
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}