using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Show Detailed Hobbler Info Trait", menuName = "Ancible Tools/Traits/Ui/Show Detailed Hobbler Info")]
    public class ShowDetailedHobblerInfoTrait : Trait
    {
        public override bool Instant => true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var showDetailHobblerInfoMsg = MessageFactory.GenerateShowDetailedHobblerInfoMsg();
            showDetailHobblerInfoMsg.Unit = _controller.transform.parent.gameObject;
            _controller.gameObject.SendMessage(showDetailHobblerInfoMsg);
            MessageFactory.CacheMessage(showDetailHobblerInfoMsg);
        }
    }
}