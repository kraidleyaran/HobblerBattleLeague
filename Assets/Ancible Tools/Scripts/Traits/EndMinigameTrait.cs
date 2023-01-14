using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "End Minigame Trait", menuName = "Ancible Tools/Traits/Minigame/End Minigame")]
    public class EndMinigameTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private MinigameResult _result = MinigameResult.Victory;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var endMinigameMsg = MessageFactory.GenerateEndMinigameMsg();
            endMinigameMsg.Result = _result;
            endMinigameMsg.Unit = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessage(endMinigameMsg);
            MessageFactory.CacheMessage(endMinigameMsg);
        }
    }
}