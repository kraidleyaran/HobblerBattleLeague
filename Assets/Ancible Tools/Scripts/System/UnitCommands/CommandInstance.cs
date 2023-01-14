using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands
{
    [Serializable]
    public class CommandInstance
    {
        public UnitCommand Command;
        public InstanceSubCommandTree Tree = new InstanceSubCommandTree();
        public bool DestroyOnClose;

        public void Destroy()
        {
            if (DestroyOnClose)
            {
                Command.Destroy();
                Object.Destroy(Command);
            }
            Tree.Destroy();
            Tree = null;
        }
    }
}