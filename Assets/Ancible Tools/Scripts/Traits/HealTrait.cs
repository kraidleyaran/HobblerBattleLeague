using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Heal Trait", menuName = "Ancible Tools/Traits/Combat/Heal")]
    public class HealTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private IntNumberRange _amount = new IntNumberRange();
        [SerializeField] private DamageType _type = DamageType.Magical;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);

            GameObject owner = _controller.transform.parent.gameObject;
            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = objOwner => owner = objOwner;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, controller.Sender);
            MessageFactory.CacheMessage(queryOwnerMsg);

            var heal = _amount.Roll();
            if (owner)
            {
                var combatStats = CombatStats.Zero;

                var queryCombatStatsMsg = MessageFactory.GenerateQueryCombatStatsMsg();
                queryCombatStatsMsg.DoAfter = (baseStats, bonusStats, genetics) =>
                {
                    combatStats = baseStats + bonusStats + genetics;
                };
                _controller.gameObject.SendMessageTo(queryCombatStatsMsg, owner);
                MessageFactory.CacheMessage(queryCombatStatsMsg);

                heal += WorldCombatController.CalculateHeal(combatStats, heal, _type);
            }

            var healMsg = MessageFactory.GenerateHealMsg();
            healMsg.Amount = heal;
            healMsg.Owner = owner;
            _controller.gameObject.SendMessageTo(healMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(healMsg);
        }

        public override string GetDescription()
        {
            return $"Heal for {_amount} ({_type})";
        }
    }
}