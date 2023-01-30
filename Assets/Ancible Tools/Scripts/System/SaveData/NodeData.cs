using System;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class NodeData : IDisposable
    {
        public string Id;
        public int Stack;

        public void Dispose()
        {
            Id = null;
            Stack = 0;
        }
    }
}