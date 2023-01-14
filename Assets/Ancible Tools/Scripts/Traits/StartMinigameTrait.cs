using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame.Settings;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Start Minigame Trait", menuName = "Ancible Tools/Traits/Minigame/Start Minigame")]
    public class StartMinigameTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private MinigameSettings _settings;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var startMinigameMsg = MessageFactory.GenerateStartMinigameMsg();
            startMinigameMsg.Owner = _controller.transform.parent.gameObject;
            startMinigameMsg.Settings = _settings;
            _controller.gameObject.SendMessage(startMinigameMsg);
            MessageFactory.CacheMessage(startMinigameMsg);
        }

        
    }
}