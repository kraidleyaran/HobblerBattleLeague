﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands
{
    public class InstanceSubCommandTree
    {
        public List<CommandInstance> SubCommands = new List<CommandInstance>();

        public void Destroy(bool destroyObj = false)
        {
            for (var i = 0; i < SubCommands.Count; i++)
            {
                if (SubCommands[i].DestroyOnClose || destroyObj)
                {
                    SubCommands[i].Destroy();
                }
            }
            SubCommands = null;
        }
    }
}