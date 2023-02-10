using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData.Building
{
    [Serializable]
    public class CraftingParameterData : BuildingParameterData
    {
        public QueuedCraftData[] Queue = new QueuedCraftData[0];

        public override void Dispose()
        {
            Queue = null;
            base.Dispose();
        }
    }
}