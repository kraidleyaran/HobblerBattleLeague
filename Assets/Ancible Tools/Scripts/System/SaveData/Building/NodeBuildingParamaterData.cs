using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData.Building
{
    [Serializable]
    public class NodeBuildingParamaterData : BuildingParameterData
    {
        public int Stack;
        public bool AutoRefillEnabled = false;

        public override void Dispose()
        {
            base.Dispose();
            Stack = 0;
            AutoRefillEnabled = false;
        }
    }
}