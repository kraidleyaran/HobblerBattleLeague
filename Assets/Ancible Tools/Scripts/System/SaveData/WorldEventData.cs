using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class WorldEventData : IDisposable
    {
        public string Name;
        public int TriggerCount;

        public void Dispose()
        {
        }
    }
}