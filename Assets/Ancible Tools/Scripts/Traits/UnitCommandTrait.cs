using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.UnitCommands;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Unit Command Trait", menuName = "Ancible Tools/Traits/Hobbler/Unit Command Trait")]
    public class UnitCommandTrait : Trait
    {
        [SerializeField] private UnitCommand[] _startingCommands = new UnitCommand[0];

        private List<CommandInstance> _commands = new List<CommandInstance>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            _commands = _startingCommands.Where(c => c).Select(c => c.GenerateInstance()).ToList();
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<QueryCommandsMessage>(QueryCommands, _instanceId);
        }

        private void QueryCommands(QueryCommandsMessage msg)
        {
            msg.DoAfter.Invoke(_commands.ToArray());
        }
    }
}