using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData.Adventure
{
    [Serializable]
    public class AdventureParameterData : IDisposable
    {
        public const string DEFAULT_FILTER = "AdventuerParameter-";

        public string Id;

        public virtual void Dispose()
        {
        }

        public virtual string GenerateFilter()
        {
            return $"{DEFAULT_FILTER}{Id}";
        }
    }
}