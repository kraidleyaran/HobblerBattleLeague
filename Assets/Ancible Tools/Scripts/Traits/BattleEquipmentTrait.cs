using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Battle Equipment Trait", menuName = "Ancible Tools/Traits/Battle/Battle Equipment")]
    public class BattleEquipmentTrait : Trait
    {
        private List<EquippableInstance> _equipped = new List<EquippableInstance>();


        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<SetEquipmentMessage>(SetEquipment, _instanceId);
        }

        private void SetEquipment(SetEquipmentMessage msg)
        {
            foreach (var item in msg.Items)
            {
                _equipped.Add(new EquippableInstance(item, _controller.transform.parent.gameObject));
            }
        }
    }
}