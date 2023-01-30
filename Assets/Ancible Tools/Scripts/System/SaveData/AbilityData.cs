using System;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class AbilityData
    {
        public string Name;
        public int Priority;

        public void Dispose()
        {
            Name = null;
            Priority = 0;
        }
    }
}