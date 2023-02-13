using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Global Cooldown Bonus Trait", menuName = "Ancible Tools/Traits/General/Apply Global Cooldown Bonus")]
    public class ApplyGlobalCooldownBonusTrait : Trait
    {
        public override bool Instant => _permanent;

        [SerializeField] private int _ticks = -1;
        [SerializeField] private bool _permanent = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applyGlobalCooldownBonusMsg = MessageFactory.GenerateApplyGlobalCooldownBonusMsg();
            applyGlobalCooldownBonusMsg.Bonus = _ticks;
            applyGlobalCooldownBonusMsg.Permanent = _permanent;
            _controller.gameObject.SendMessageTo(applyGlobalCooldownBonusMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyGlobalCooldownBonusMsg);
        }

        public override void Destroy()
        {
            if (!_permanent)
            {
                var applyGlobalCooldownBonusMsg = MessageFactory.GenerateApplyGlobalCooldownBonusMsg();
                applyGlobalCooldownBonusMsg.Bonus = _ticks * -1;
                applyGlobalCooldownBonusMsg.Permanent = false;
                _controller.gameObject.SendMessageTo(applyGlobalCooldownBonusMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(applyGlobalCooldownBonusMsg);
            }
            base.Destroy();
        }
    }
}