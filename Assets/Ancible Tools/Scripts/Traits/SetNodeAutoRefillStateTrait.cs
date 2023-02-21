using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Set Node Auto Refill State Trait", menuName = "Ancible Tools/Traits/Node/Set Node Auto Refill State")]
    public class SetNodeAutoRefillStateTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private bool _state = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var setNodeAutoRefillStateMsg = MessageFactory.GenerateSetNodeAutoRefillStateMsg();
            setNodeAutoRefillStateMsg.AutoRefill = _state;
            _controller.gameObject.SendMessageTo(setNodeAutoRefillStateMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(setNodeAutoRefillStateMsg);
        }
    }
}