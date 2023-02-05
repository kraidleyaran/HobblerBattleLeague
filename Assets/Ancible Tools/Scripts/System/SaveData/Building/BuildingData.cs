using System;
using Assets.Resources.Ancible_Tools.Scripts.System.SaveData;

namespace Assets.Ancible_Tools.Scripts.System.SaveData.Buildings
{
    [Serializable]
    public class BuildingData : IDisposable
    {
        public string Id;
        public string Building;
        public Vector2IntData Position;
        public BuildingParameterData Parameter = null;

        public void Dispose()
        {
            Id = null;
            Building = null;
            Position = Vector2IntData.Zero;
        }
    }
}