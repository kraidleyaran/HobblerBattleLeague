using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Add Experience Trait", menuName = "Ancible Tools/Traits/Hobbler/Leveling/Add Experience")]
    public class AddExperienceTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _amount = 1;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var addExperienceMsg = MessageFactory.GenerateAddExperienceMsg();
            addExperienceMsg.Amount = _amount;
            _controller.gameObject.SendMessageTo(addExperienceMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(addExperienceMsg);
        }

        
    }
}