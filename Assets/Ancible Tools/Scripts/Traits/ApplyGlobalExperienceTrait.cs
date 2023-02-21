using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Global Experience Trait", menuName = "Ancible Tools/Traits/Hobbler/Leveling/Apply Global Experience")]
    public class ApplyGlobalExperienceTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private int _amount = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var queryExperiencePoolMsg = MessageFactory.GenerateQueryExperiencePoolMsg();
            queryExperiencePoolMsg.DoAfter = ApplyExperience;
            _controller.gameObject.SendMessageTo(queryExperiencePoolMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(queryExperiencePoolMsg);
        }

        private void ApplyExperience(int experiencePool)
        {
            if (experiencePool > 0)
            {
                var amount = Mathf.Min(_amount, experiencePool);
                var applyGlobalExperienceMsg = MessageFactory.GenerateApplyGlobalExperienceMsg();
                applyGlobalExperienceMsg.Amount = amount;
                applyGlobalExperienceMsg.Owner = _controller.transform.parent.gameObject;
                _controller.gameObject.SendMessage(applyGlobalExperienceMsg);
                MessageFactory.CacheMessage(applyGlobalExperienceMsg);
            }
        }
    }
}