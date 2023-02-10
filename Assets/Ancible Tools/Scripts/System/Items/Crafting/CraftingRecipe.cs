using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Items.Crafting
{
    [CreateAssetMenu(fileName = "Crafting Recipe", menuName = "Ancible Tools/Items/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public ItemStack Item = null;
        public ItemStack[] RequiredItems = new ItemStack[0];
        public int Cost = 0;
        public int CraftingTicks = 0;

        public void RemoveCost(int amount)
        {
            WorldStashController.RemoveGold(Cost * amount);
            if (RequiredItems.Length > 0)
            {
                foreach (var item in RequiredItems)
                {
                    WorldStashController.RemoveItem(item.Item, item.Stack * amount);
                }
            }
        }

        public void Refund(int amount)
        {
            WorldStashController.AddGold(Cost * amount);
            if (RequiredItems.Length > 0)
            {
                foreach (var item in RequiredItems)
                {
                    WorldStashController.AddItem(item.Item, item.Stack * amount);
                }
            }
        }
    }
}