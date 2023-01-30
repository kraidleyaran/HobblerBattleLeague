using System.Collections.Generic;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Factories;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Hobbler Ability Manager Trait", menuName = "Ancible Tools/Traits/Hobbler/Hobbler Ability Manager")]
    public class HobblerAbilityManagerTrait : Trait
    {
        [SerializeField] private int _maxAbilitySlots = 4;
        [SerializeField] private WorldAbility[] _startingAbilities = new WorldAbility[0];

        private Dictionary<int, WorldAbility> _abilities = new Dictionary<int, WorldAbility>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            for (var i = 0; i < _maxAbilitySlots; i++)
            {
                _abilities.Add(i, null);
            }

            for (var i = 0; i < _startingAbilities.Length && i < _maxAbilitySlots; i++)
            {
                _abilities[i] = _startingAbilities[i];
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryAbilitiesMessage>(QueryAbilities, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ChangeAbilitySlotMessage>(ChangeAbilitySlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<LearnAbilityMessage>(LearnAbility, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ForgetAbilityAtSlotMessage>(ForgetAbilityAtSlot, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAbilitiesMessage>(SetAbilities, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAbilitiesFromDataMessage>(SetAbilitiesFromData, _instanceId);
        }

        private void QueryAbilities(QueryAbilitiesMessage msg)
        {
            msg.DoAfter.Invoke(_abilities.ToArray());
        }

        private void ChangeAbilitySlot(ChangeAbilitySlotMessage msg)
        {
            if (_abilities.TryGetValue(msg.CurrentSlot, out var currentAbility) && _abilities.TryGetValue(msg.NewSlot, out var toAbility))
            {
                _abilities[msg.CurrentSlot] = toAbility;
                _abilities[msg.NewSlot] = currentAbility;

                _controller.gameObject.SendMessageTo(AbilitiesUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void LearnAbility(LearnAbilityMessage msg)
        {
            if (_abilities.TryGetValue(msg.Slot, out var currentAbility) && _abilities.Values.FirstOrDefault(a => a == msg.Ability) == null)
            {
                _abilities[msg.Slot] = msg.Ability;
                _controller.gameObject.SendMessageTo(AbilitiesUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void ForgetAbilityAtSlot(ForgetAbilityAtSlotMessage msg)
        {
            if (_abilities.TryGetValue(msg.Slot, out var currentAbility))
            {
                _abilities[msg.Slot] = null;
                _controller.gameObject.SendMessageTo(AbilitiesUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
            }
        }

        private void SetAbilities(SetAbilitiesMessage msg)
        {
            var abilityKeys = _abilities.Keys.ToArray();
            for (var i = 0; i < abilityKeys.Length; i++)
            {
                _abilities[abilityKeys[i]] = null;
            }

            for (var i = 0; i < msg.Abilities.Length && i < abilityKeys.Length; i++)
            {
                _abilities[abilityKeys[i]] = msg.Abilities[i];
            }
            _controller.gameObject.SendMessageTo(AbilitiesUpdatedMessage.INSTANCE, _controller.transform.parent.gameObject);
        }

        private void SetAbilitiesFromData(SetAbilitiesFromDataMessage msg)
        {
            var abilityKeys = _abilities.Keys.ToArray();
            for (var i = 0; i < abilityKeys.Length; i++)
            {
                _abilities[abilityKeys[i]] = null;
            }

            foreach (var ability in msg.Abilities)
            {
                if (_abilities.Keys.Contains(ability.Priority))
                {
                    var worldAbility = WorldAbilityFactory.GetAbilityByName(ability.Name);
                    if (worldAbility)
                    {
                        _abilities[ability.Priority] = worldAbility;
                    }
                }

            }
        }
    }
}