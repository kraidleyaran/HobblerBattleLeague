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
            var description = string.Empty;
            var traitDescriptions = _applyOnEquip.Select(t => t.GetDescription()).Where(d => !string.IsNullOrWhiteSpace(d)).ToArray();
            for (var i = 0; i < traitDescriptions.Length; i++)
            {
                description = string.IsNullOrEmpty(description) ? $"{traitDescriptions[i]}" : $"{description}{Environment.NewLine}{traitDescriptions[i]}";
            }

            if (!string.IsNullOrEmpty(Description))
            {
                description = $"{description}{Environment.NewLine}{Description}";
            }
            return description;
        }
    }
}