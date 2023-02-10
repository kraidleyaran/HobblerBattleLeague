using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData.Building
{
    [Serializable]
    public class HobGateParameterData : BuildingParameterData
    {
        public string[] Hobs = new string[0];
        public int RemainingTicks = 0;
        public int MaxTicks = 0;

        public override void Dispose()
        {
            Hobs = null;
            RemainingTicks = 0;
            MaxTicks = 0;
            base.Dispose();
        }
    }
}