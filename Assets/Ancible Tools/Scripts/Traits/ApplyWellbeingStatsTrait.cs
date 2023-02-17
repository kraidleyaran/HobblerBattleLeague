using Assets.Ancible_Tools.Scripts.System.Wellbeing;
using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Wellbeing Stats Trait", menuName = "Ancible Tools/Traits/Stats/Wellbeing/Apply Wellbeing Stats")]
    public class ApplyWellbeingStatsTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private WellbeingStats _stats = new WellbeingStats();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applyWellbeingStatsMsg = MessageFactory.GenerateApplyWellbeingStatsMsg();
            applyWellbeingStatsMsg.Stats = _stats;
            _controller.gameObject.SendMessageTo(applyWellbeingStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyWellbeingStatsMsg);
        }
    }
}