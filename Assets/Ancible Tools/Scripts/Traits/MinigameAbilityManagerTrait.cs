using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Miningame Ability Manager Trait", menuName = "Ancible Tools/Traits/Minigame/Combat/Minigame Ability Manager")]
    public class MinigameAbilityManagerTrait : Trait
    {
        [SerializeField] private WorldAbility[] _startingAbilities = new WorldAbility[0];

        private Dictionary<int, AbilityInstance> _abilities = new Dictionary<int, AbilityInstance>();

        

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            for (var i = 0; i < _startingAbilities.Length; i++)
            {
                _abilities.Add(i, _startingAbilities[i].GenerateInstance());
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryMinigameAbilitiesMessage>(QueryMinigameAbilities, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetAbilitiesMessage>(SetAbilities, _instanceId);
        }

        private void QueryMinigameAbilities(QueryMinigameAbilitiesMessage msg)
        {
            msg.DoAfter.Invoke(_abilities.ToArray());
        }

        private void SetAbilities(SetAbilitiesMessage msg)
        {
            _abilities.Clear();
            for (var i = 0; i < msg.Abilities.Length; i++)
            {
                _abilities.Add(i, msg.Abilities[i] ? new AbilityInstance(msg.Abilities[i]) : null);
            }
        }
    }
}