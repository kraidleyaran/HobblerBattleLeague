using System;
using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands
{
    [CreateAssetMenu(fileName = "Unit Command", menuName = "Ancible Tools/Unit Command")]
    public class UnitCommand : ScriptableObject
    {
        public string Command;
        [TextArea(3, 10)] public string Description;
        public CommandIcon[] Icons = new CommandIcon[0];
        public Trait[] OnCommand = new Trait[0];
        public SubCommandTree Tree = new SubCommandTree();
        public Action DoAfter = null;

        public void ApplyCommand(GameObject obj)
        {
            if (OnCommand.Length > 0)
            {
                var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                for (var i = 0; i < OnCommand.Length; i++)
                {
                    addTraitToUnitMsg.Trait = OnCommand[i];
                    obj.SendMessageTo(addTraitToUnitMsg, obj);
                }
                MessageFactory.CacheMessage(addTraitToUnitMsg);
            }
            DoAfter?.Invoke();
        }

        public CommandInstance GenerateInstance()
        {
            return new CommandInstance {Command = this, Tree = Tree.GenerateInstance()};
        }

        public void Destroy()
        {
            Icons = null;
            OnCommand = null;
            Tree.Destroy();
            Tree = null;
            DoAfter = null;
        }
    }
}