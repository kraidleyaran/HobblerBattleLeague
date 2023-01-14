using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Refill Node Stacks Trait", menuName = "Ancible Tools/Traits/Node/Refill Node Stacks")]
    public class RefillNodeStacksTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _stacks = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var refillNodeStacksMsg = MessageFactory.GenerateRefillNodeStacksMsg();
            refillNodeStacksMsg.Max = _stacks;
            _controller.gameObject.SendMessageTo(refillNodeStacksMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(refillNodeStacksMsg);
        }
    }
}