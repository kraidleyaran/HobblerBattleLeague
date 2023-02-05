using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands
{
    [Serializable]
    public class CommandInstance
    {
        public UnitCommand Command;
        public InstanceSubCommandTree Tree;
        public bool DestroyOnClose;

        public CommandInstance()
        {
            Tree = new InstanceSubCommandTree();
        }

        public void Destroy(bool destroyObj = false)
        {
            if (DestroyOnClose || destroyObj)
            {
                Command.Destroy();
                Object.Destroy(Command);
            }
            Tree?.Destroy();
            Tree = null;
        }
    }
}