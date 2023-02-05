using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Refill Node Stacks Trait", menuName = "Ancible Tools/Traits/Node/Refill Node Stacks")]
    public class RefillNodeStacksTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _stacks = 1;
        [SerializeField] private int _refillCost = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (WorldStashController.Gold >= _refillCost)
            {
                WorldStashController.RemoveGold(_refillCost);
                var refillNodeStacksMsg = MessageFactory.GenerateRefillNodeStacksMsg();
                refillNodeStacksMsg.Max = _stacks;
                _controller.gameObject.SendMessageTo(refillNodeStacksMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(refillNodeStacksMsg);
            }
            else
            {
                UiOverlayTextManager.ShowOverlayAlert("Not enough gold", ColorFactoryController.ErrorAlertText);
            }

        }
    }
}