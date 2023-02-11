using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Absorb Damage Trait", menuName = "Ancible Tools/Traits/Combat/Absorb Damage")]
    public class AbsorbDamageTrait : Trait
    {
        [SerializeField] private int _amount;
        [SerializeField] private DamageType _type;
        [SerializeField] private Trait[] _applyOnEffect = new Trait[0];

        private int _remainingDamage = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _remainingDamage = _amount;
            SubscribeToMessages();
        }

        private void AbsorbDamage(WorldInstance<int> damageInstance)
        {

            if (_remainingDamage <= damageInstance.Instance)
            {
                damageInstance.Instance -= _remainingDamage;
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                removeTraitFromUnitByControllerMsg.Controller = _controller;
                _controller.gameObject.SendMessageTo(removeTraitFromUnitByControllerMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
            }
            else
            {
                _remainingDamage -= damageInstance.Instance;
                damageInstance.Instance = 0;
            }

            if (_applyOnEffect.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                foreach (var trait in _applyOnEffect)
                {
                    addTraitToUnitMsg.Trait = trait;
                    _controller.transform.parent.gameObject.SendMessageTo(addTraitToUnitMsg, _controller.transform.parent.gameObject);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<AbsorbedDamageCheckMessage>(AbsorbDamageCheck, _instanceId);
        }

        private void AbsorbDamageCheck(AbsorbedDamageCheckMessage msg)
        {
            if (msg.Instance.Instance > 0)
            {
                switch (_type)
                {
                    case DamageType.Physical:
                    case DamageType.Magical:
                        if (msg.Type == _type)
                        {
                            AbsorbDamage(msg.Instance);
                        }
                        break;
                    case DamageType.Pure:
                        AbsorbDamage(msg.Instance);
                        break;
                }
            }
        }
    }
}