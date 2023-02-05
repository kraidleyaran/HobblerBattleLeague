using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Maze Selection Window Trait", menuName = "Ancible Tools/Traits/Ui/Show Maze Selection Window")]
    public class ShowMazeSelectionWindowTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var showMazeSelectionWindowMsg = MessageFactory.GenerateShowMazeSelectionWindowMsg();
            showMazeSelectionWindowMsg.Hobbler = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessage(showMazeSelectionWindowMsg);
            MessageFactory.CacheMessage(showMazeSelectionWindowMsg);
        }
    }
}