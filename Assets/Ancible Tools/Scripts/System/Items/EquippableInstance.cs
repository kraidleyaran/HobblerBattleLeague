using System;
using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class EquippableInstance : WorldInstance<EquippableItem>
    {
        private GameObject _owner = null;
        private TraitController[] _appliedTraits = new TraitController[0];

        public EquippableInstance(EquippableItem item, GameObject owner)
        {
            Instance = item;
            _owner = owner;
            if (Instance.ApplyOnEquip.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                var appliedTraits = new List<TraitController>();
                for (var i = 0; i < Instance.ApplyOnEquip.Length; i++)
                {
                    addTraitToUnitMsg.Trait = Instance.ApplyOnEquip[i];
                    if (Instance.ApplyOnEquip[i].Instant)
                    {
                        addTraitToUnitMsg.DoAfter = controller => { };
                    }
                    else
                    {
                        addTraitToUnitMsg.DoAfter = controller => appliedTraits.Add(controller);
                    }
                    _owner.SendMessageTo(addTraitToUnitMsg, owner);
                }

                _appliedTraits = appliedTraits.ToArray();
            }

            if (Instance.Slot == EquipSlot.Weapon && Instance is WeaponItem weapon)
            {
                var setBasictAttackSetupMsg = MessageFactory.GenerateSetBasicAttackSetupMsg();
                setBasictAttackSetupMsg.Setup = weapon.AttackSetup;
                _owner.SendMessageTo(setBasictAttackSetupMsg, _owner);
                MessageFactory.CacheMessage(setBasictAttackSetupMsg);
            }
            
        }

        public override void Destroy()
        {
            if (_appliedTraits.Length > 0)
            {
                var removeTraitFromUnitByControllerMsg = MessageFactory.GenerateRemoveTraitFromUnitByControllerMsg();
                for (var i = 0; i < _appliedTraits.Length; i++)
                {
                    removeTraitFromUnitByControllerMsg.Controller = _appliedTraits[i];
                    _owner.SendMessageTo(removeTraitFromUnitByControllerMsg, _owner);
                }
                MessageFactory.CacheMessage(removeTraitFromUnitByControllerMsg);
                _appliedTraits = new TraitController[0];
            }

            if (Instance.Slot == EquipSlot.Weapon && Instance is WeaponItem weapon)
            {
                var clearBasicAttackSetupMsg = MessageFactory.GenearteClearBasicAttackSetupMsg();
                clearBasicAttackSetupMsg.Setup = weapon.AttackSetup;
                _owner.SendMessageTo(clearBasicAttackSetupMsg, _owner);
                MessageFactory.CacheMessage(clearBasicAttackSetupMsg);
            }

            _owner = null;
            Instance = null;
            base.Destroy();
        }
    }
}