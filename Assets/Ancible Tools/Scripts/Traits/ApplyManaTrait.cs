using System;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Mana Trait", menuName = "Ancible Tools/Traits/Combat/Apply Mana")]
    public class ApplyManaTrait : Trait
    {
        public override bool Instant => true;
        [SerializeField] private FloatNumberRange _amount = FloatNumberRange.Zero;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applyManaMsg = MessageFactory.GenerateApplyManaMsg();
            applyManaMsg.Amount = _amount.Roll();
            _controller.gameObject.SendMessageTo(applyManaMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyManaMsg);
        }

        public override string GetDescription(bool equipment = false)
        {
            return $"Regenerates {_amount} Mana";
        }
    }
}