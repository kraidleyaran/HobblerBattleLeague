using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public struct QueuedCraftData : IDisposable
    {
        public string Recipe;
        public int Count;
        public int RemainingTicks;

        public void Dispose()
        {
            Recipe = null;
            Count = 0;
            RemainingTicks = 0;
        }
    }
}