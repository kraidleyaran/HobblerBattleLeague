using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class WorldEventReceiverData : IDisposable
    {
        public string Id;
        public int TriggerCount = 0;

        public void Dispose()
        {

        }
    }
}