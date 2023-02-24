using System;
using System.Linq;
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
            var description = $"{(Quality != ItemQuality.Basic ? $"{Quality.ToColorString()}{Environment.NewLine}" : string.Empty)}Equippable - {Slot}{StaticMethods.DoubleNewLine()}{AttackSetup.GetDescription()}{Environment.NewLine}";
            description = GetEquippedDescription(description);
            if (!string.IsNullOrEmpty(Description))
            {
                description = $"{description}{Environment.NewLine}{Description}";
            }
            return description;
        }
    }
}