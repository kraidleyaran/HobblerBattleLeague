using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Damage Trait", menuName = "Ancible Tools/Traits/Combat/Damage")]
    public class DamageTrait : Trait
    {
        public override bool Instant => true;
        public override int DescriptionPriority => 100;
        
        [SerializeField] private IntNumberRange _amount = new IntNumberRange();
        [SerializeField] private DamageType _type = DamageType.Physical;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            GameObject owner = null;

            var queryOwnerMsg = MessageFactory.GenerateQueryOwnerMsg();
            queryOwnerMsg.DoAfter = obj => owner = obj;
            _controller.gameObject.SendMessageTo(queryOwnerMsg, _controller.Sender);
            MessageFactory.CacheMessage(queryOwnerMsg);
            var bonus = 0;
            if (owner)
            {
                var queryBonusDamageMsg = MessageFactory.GenerateQueryBonusDamageMsg();
                queryBonusDamageMsg.DoAfter = damage => bonus += damage;
                queryBonusDamageMsg.Type = _type;
                _controller.gameObject.SendMessageTo(queryBonusDamageMsg, owner);
                MessageFactory.CacheMessage(queryBonusDamageMsg);
            }
            else
            {
                owner = _controller.gameObject;
            }
            

            var damageMsg = MessageFactory.GenerateDamageMsg();
            damageMsg.Owner = owner;
            damageMsg.Amount = _amount.Roll() + bonus;
            _controller.gameObject.SendMessageTo(damageMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(damageMsg);

        }

        public override string GetDescription()
        {
            return $"{_amount} {_type} Damage";
        }
    }
}