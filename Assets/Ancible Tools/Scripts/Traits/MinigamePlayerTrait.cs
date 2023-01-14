using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Minigame Player Trait", menuName = "Ancible Tools/Traits/Minigame/Player/Minigame Player")]
    public class MinigamePlayerTrait : Trait
    {
        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<UnitDiedMessage>(UnitDied, _instanceId);
        }

        private void UnitDied(UnitDiedMessage msg)
        {
            var endMinigameMsg = MessageFactory.GenerateEndMinigameMsg();
            endMinigameMsg.Result = MinigameResult.Defeat;
            endMinigameMsg.Unit = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessage(endMinigameMsg);
            MessageFactory.CacheMessage(endMinigameMsg);
        }
    }
}