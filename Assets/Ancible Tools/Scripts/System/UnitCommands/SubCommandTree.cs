using System;
using System.Linq;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands
{
    [Serializable]
    public class SubCommandTree
    {
        public UnitCommand[] SubCommands = new UnitCommand[0];

        public InstanceSubCommandTree GenerateInstance()
        {
            return new InstanceSubCommandTree { SubCommands = SubCommands.Where(s => s).Select(s => s.GenerateInstance()).ToList()};
        }

        public void Destroy()
        {
            SubCommands = null;
        }
    }
}