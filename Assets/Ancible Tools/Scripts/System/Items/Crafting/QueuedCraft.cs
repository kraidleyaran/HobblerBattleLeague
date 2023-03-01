using System;
using Assets.Ancible_Tools.Scripts.System.SaveData;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;

namespace Assets.Ancible_Tools.Scripts.System.Items.Crafting
{
    public class QueuedCraft : IDisposable
    {
        public CraftingRecipe Recipe;
        public int Count;
        public int RemainingTicks { get; private set; }

        public QueuedCraft(CraftingRecipe recipe, int count)
        {
            Recipe = recipe;
            Count = count;
            RemainingTicks = recipe.CraftingTicks;
        }

        public void SetRemainingTicks(int ticks)
        {
            RemainingTicks = ticks;
        }

        public bool Tick()
        {
            RemainingTicks--;
            if (RemainingTicks <= 0)
            {
                Count--;
                WorldStashController.AddItem(Recipe.Item.Item, Recipe.Item.Stack);
                if (Count <= 0)
                {
                    return true;
                }

                RemainingTicks = Recipe.CraftingTicks;
            }

            return false;
        }

        public void Destroy()
        {
            Dispose();
        }

        public QueuedCraftData GetData()
        {
            return new QueuedCraftData {Count = Count, Recipe = Recipe.name, RemainingTicks = RemainingTicks};
        }


        public void Dispose()
        {
            Recipe = null;
            Count = 0;
            RemainingTicks = 0;
        }
    }
}