using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Hob Generator Window Trait", menuName = "Ancible Tools/Traits/Ui/Show Hob Generator Window")]
    public class ShowHobGeneratorWindowTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var showHobGeneratorWindowMsg = MessageFactory.GenerateShowHobGeneratorWindowMsg();
            showHobGeneratorWindowMsg.Owner = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessage(showHobGeneratorWindowMsg);
            MessageFactory.CacheMessage(showHobGeneratorWindowMsg);
        }
    }
}