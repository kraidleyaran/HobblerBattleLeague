using System;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Armor Item", menuName = "Ancible Tools/Items/Equippable/Armor Item")]
    public class EquippableItem : WorldItem
    {
        public override WorldItemType Type => WorldItemType.Equippable;
        public virtual EquipSlot Slot => EquipSlot.Armor;
        public Trait[] ApplyOnEquip => _applyOnEquip;

        [SerializeField] private Trait[] _applyOnEquip = new Trait[0];

        public override string GetDescription()
        {
            var description = $"{(Quality != ItemQuality.Basic ? $"{Quality.ToColorString()}{Environment.NewLine}" : string.Empty)}Equippable - {Slot}{Environment.NewLine}";
            description = GetEquippedDescription(description);
            if (!string.IsNullOrEmpty(Description))
            {
                description = $"{description}{Environment.NewLine}{Description}";
            }
            return description;
        }

        protected internal string GetEquippedDescription(string description)
        {
            if (_applyOnEquip.Length > 0)
            {
                var traitDescriptions = _applyOnEquip.Select(t => t.GetDescription()).Where(d => !string.IsNullOrWhiteSpace(d)).ToArray();
                var returnDescription = description;
                var startDescription = $"{StaticMethods.ApplyColorToText("On Equip:", ColorFactoryController.BonusStat)}";
                returnDescription = string.IsNullOrEmpty(returnDescription) ? startDescription : $"{returnDescription}{Environment.NewLine}{startDescription}";
                foreach (var trait in traitDescriptions)
                {
                    returnDescription = $"{returnDescription}{Environment.NewLine}{trait}";
                }
                return returnDescription;
            }

            return description;
        }
    }
}