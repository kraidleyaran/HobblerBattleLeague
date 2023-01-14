using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Weapon Item", menuName = "Ancible Tools/Items/Equippable/Weapon Item")]
    public class WeaponItem : EquippableItem
    {
        public override EquipSlot Slot => EquipSlot.Weapon;
        public BasicAttackSetup AttackSetup = new BasicAttackSetup();

        public override string GetDescription()
        {
            return $"{AttackSetup.GetDescription()}{Environment.NewLine}{base.GetDescription()}";
        }
    }
}