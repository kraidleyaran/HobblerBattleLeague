using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class TrainerData : IDisposable
    {
        public string Id;

        public void Dispose()
        {
            Id = null;
        }
    }
}