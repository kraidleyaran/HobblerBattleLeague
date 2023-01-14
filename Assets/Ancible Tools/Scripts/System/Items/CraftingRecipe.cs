using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class CraftingRecipe : ScriptableObject
    {
        public WorldItem Item = null;
        public ItemStack[] RequiredItems = new ItemStack[0];
        public int RequiredTicks = 0;
        public WorldSkill Skill = null;
        public int RequiredLevel = 0;
    }
}