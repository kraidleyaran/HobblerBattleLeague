using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Ai Ability Manager Trait", menuName = "Ancible Tools/Traits/Minigame/Ai/Minigame Ai Ability Manager")]
    public class MinigameAiAbilityManagerTrait : Trait
    {
        [SerializeField] private AiAbility[] _startingAbilities = new AiAbility[0];

        private Dictionary<AbilityInstance, int> _abilities = new Dictionary<AbilityInstance, int>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            for (var i = 0; i < _startingAbilities.Length; i++)
            {
                _abilities.Add(_startingAbilities[i].Ability.GenerateInstance(), _startingAbilities[i].Priority);
            }
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<BattleAbilityCheckMessage>(BattleAbilityCheck, _instanceId);
        }

        private void BattleAbilityCheck(BattleAbilityCheckMessage msg)
        {
            var canCast = true;
            var castCheckMsg = MessageFactory.GenerateCanCastCheckMsg();
            castCheckMsg.DoAfter = () => canCast = false;
            _controller.gameObject.SendMessageTo(castCheckMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(castCheckMsg);

            if (canCast)
            {
                var availableAbilities = _abilities.Where(kv => !kv.Key.OnCooldown && kv.Key.Instance.CanApplyToTarget(_controller.transform.parent.gameObject, msg.Target, msg.Distance, false)).ToArray();
                if (availableAbilities.Length > 0)
                {
                    var orderedAbilities = availableAbilities.OrderByDescending(kv => kv.Value).ToArray();
                    var equalAbilities = orderedAbilities.Where(kv => kv.Value == orderedAbilities[0].Value).ToArray();
                    var ability = equalAbilities.GetRandom();
                    var castAbilityMsg = MessageFactory.GenerateCastAbilityMsg();
                    castAbilityMsg.Ability = ability.Key;
                    castAbilityMsg.Target = msg.Target;
                    _controller.gameObject.SendMessageTo(castAbilityMsg, _controller.transform.parent.gameObject);
                    MessageFactory.CacheMessage(castAbilityMsg);
                    msg.DoAfter.Invoke();
                }
            }

        }
    }
}