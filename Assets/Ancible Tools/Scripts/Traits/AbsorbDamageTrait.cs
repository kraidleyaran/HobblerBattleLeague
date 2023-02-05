using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    public class AbsorbDamageTrait : Trait
    {
        [SerializeField] private int _amount;
        [SerializeField] private DamageType _type;

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
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<AbsorbedDamageCheckMessage>(AbsorbDamageCheck, _instanceId);
        }

        private void AbsorbDamageCheck(AbsorbedDamageCheckMessage msg)
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