using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [CreateAssetMenu(fileName = "Trinket Item", menuName = "Ancible Tools/Items/Equippable/Trinket Item")]
    public class TrinketItem : EquippableItem
    {
        public override EquipSlot Slot => EquipSlot.Trinket;
    }
}