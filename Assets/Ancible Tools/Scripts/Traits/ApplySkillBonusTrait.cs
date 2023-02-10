using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Skills;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Skill Bonus Trait", menuName = "Ancible Tools/Traits/Hobbler/Skills/Apply Skill Bonus")]
    public class ApplySkillBonusTrait : Trait
    {
        public override bool Instant => _permanent;

        [SerializeField] private float _amount = 1;
        [SerializeField] private WorldSkill _skill = null;
        [SerializeField] private bool _permanent = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applySkillBonusMsg = MessageFactory.GenerateApplySkillBonusMsg();
            applySkillBonusMsg.Skill = _skill;
            applySkillBonusMsg.Bonus = _amount;
            applySkillBonusMsg.Permanent = _permanent;
            _controller.gameObject.SendMessageTo(applySkillBonusMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applySkillBonusMsg);
        }
        

        public override void Destroy()
        {
            if (!_permanent)
            {
                var applySkillBonusMsg = MessageFactory.GenerateApplySkillBonusMsg();
                applySkillBonusMsg.Skill = _skill;
                applySkillBonusMsg.Bonus = _amount * -1;
                applySkillBonusMsg.Permanent = false;
                _controller.gameObject.SendMessageTo(applySkillBonusMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(applySkillBonusMsg);
            }
            base.Destroy();
        }
    }
}