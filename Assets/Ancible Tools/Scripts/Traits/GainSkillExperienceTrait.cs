using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Gain Skill Experience Trait", menuName = "Ancible Tools/Traits/Hobbler/Skills/Gain Skill Experience")]
    public class GainSkillExperienceTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private WorldSkill _skill = null;
        [SerializeField] private int _experience = 0;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var gainSkillExperienceMsg = MessageFactory.GenerateGainSkillExperienceMsg();
            gainSkillExperienceMsg.Skill = _skill;
            gainSkillExperienceMsg.Experience = _experience;
            _controller.gameObject.SendMessageTo(gainSkillExperienceMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(gainSkillExperienceMsg);
        }
    }
}