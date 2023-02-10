using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData.Building
{
    [Serializable]
    public class NodeBuildingParamaterData : BuildingParameterData
    {
        public int Stack;

        public override void Dispose()
        {
            base.Dispose();
            Stack = 0;
        }
    }
}