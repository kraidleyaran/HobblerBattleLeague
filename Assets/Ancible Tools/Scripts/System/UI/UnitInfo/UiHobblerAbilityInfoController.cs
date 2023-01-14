using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.DragDrop;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.UI.UnitInfo
{
    public class UiHobblerAbilityInfoController : MonoBehaviour
    {
        private const string FILTER = "UI_HOBBLER_ABILITY_INFO";

        [SerializeField] private UiAbilitySlotController[] _abilityControllers = new UiAbilitySlotController[0];

        private GameObject _unit = null;

        private UiAbilitySlotController _hovered = null;

        public void Setup(GameObject unit)
        {
            _unit = unit;
            _unit.gameObject.SubscribeWithFilter<AbilitiesUpdatedMessage>(AbilitiesUpdated, FILTER);
            for (var i = 0; i < _abilityControllers.Length; i++)
            {
                _abilityControllers[i].WakeUp(gameObject);
            }
            RefreshInfo();
            SubscribeToMessages();
        }

        public void RefreshInfo()
        {
            var queryAbilitiesMsg = MessageFactory.GenerateQueryAbilitiesMsg();
            queryAbilitiesMsg.DoAfter = RefreshAbilities;
            gameObject.SendMessageTo(queryAbilitiesMsg, _unit);
            MessageFactory.CacheMessage(queryAbilitiesMsg);
        }

        private void RefreshAbilities(KeyValuePair<int, WorldAbility>[] abilities)
        {
            for (var i = 0; i < _abilityControllers.Length; i++)
            {
                _abilityControllers[i].Setup(null, _unit);
            }
            for (var i = 0; i < abilities.Length && i < _abilityControllers.Length; i++)
            {
                _abilityControllers[i].Setup(abilities[i].Value, _unit);
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.SubscribeWithFilter<SetHoveredAbilitySlotControllerMessage>(SetHoveredAbilitySlot, FILTER);
            gameObject.SubscribeWithFilter<RemoveHoveredAbilitySlotControllerMessage>(RemoveHoveredAbilitySlot, FILTER);
            gameObject.SubscribeWithFilter<ReceiveDragDropItemMessage>(ReceiveDragDropItem, FILTER);
        }

        private void AbilitiesUpdated(AbilitiesUpdatedMessage msg)
        {
            RefreshInfo();
        }

        private void SetHoveredAbilitySlot(SetHoveredAbilitySlotControllerMessage msg)
        {
            if (!_hovered || _hovered != msg.Controller)
            {
                _hovered = msg.Controller;
            }
        }

        private void RemoveHoveredAbilitySlot(RemoveHoveredAbilitySlotControllerMessage msg)
        {
            if (_hovered && _hovered == msg.Controller)
            {
                _hovered = null;
            }
        }

        private void ReceiveDragDropItem(ReceiveDragDropItemMessage msg)
        {
            if (_hovered && msg.Item.Type == WorldItemType.Ability && msg.Item is AbilityItem ability)
            {
                var existingAbility = _abilityControllers.FirstOrDefault(c => c.Ability == ability.Ability);
                if (!existingAbility)
                {
                    var learnAbilityMsg = MessageFactory.GenerateLearnAbilityMsg();
                    learnAbilityMsg.Ability = ability.Ability;
                    learnAbilityMsg.Slot = _hovered.transform.parent.GetSiblingIndex();
                    gameObject.SendMessageTo(learnAbilityMsg, _unit);
                    MessageFactory.CacheMessage(learnAbilityMsg);

                    var removeItemMsg = MessageFactory.GenerateRemoveItemMsg();
                    removeItemMsg.Item = msg.Item;
                    removeItemMsg.Stack = 1;
                    gameObject.SendMessageTo(removeItemMsg, msg.Owner);
                    MessageFactory.CacheMessage(removeItemMsg);

                    msg.DoAfter.Invoke();
                }
                
            }
        }

        public void Destroy()
        {
            _unit.UnsubscribeFromAllMessagesWithFilter(FILTER);
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}