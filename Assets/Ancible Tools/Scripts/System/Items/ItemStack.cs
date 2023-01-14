using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    [Serializable]
    public class ItemStack
    {
        public WorldItem Item;
        public int Stack;

        public ItemStack Clone()
        {
            return new ItemStack {Item = Item, Stack = Stack};
        }

        public void Destroy()
        {
            Item = null;
            Stack = 0;
        }
    }
}