using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Apply Combat Stats Trait", menuName = "Ancible Tools/Traits/Combat/Apply Combat Stats")]
    public class ApplyCombatStatsTrait : Trait
    {
        public override bool Instant => !_permanent;

        [SerializeField] private CombatStats _stats = CombatStats.Zero;
        [SerializeField] private bool _permanent = false;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            var applyCombatStatsMsg = MessageFactory.GenerateApplyCombatStatsMsg();
            applyCombatStatsMsg.Stats = _stats;
            applyCombatStatsMsg.Bonus = !_permanent;
            _controller.gameObject.SendMessageTo(applyCombatStatsMsg, _controller.transform.parent.gameObject);
            MessageFactory.CacheMessage(applyCombatStatsMsg);
        }

        public override string GetDescription(bool equipment = false)
        {
            return _stats.GetDescription(false, equipment);
        }

        public override void Destroy()
        {
            if (!_permanent)
            {
                var applyCombatStatsMsg = MessageFactory.GenerateApplyCombatStatsMsg();
                applyCombatStatsMsg.Stats = _stats * -1;
                applyCombatStatsMsg.Bonus = !_permanent;
                _controller.gameObject.SendMessageTo(applyCombatStatsMsg, _controller.transform.parent.gameObject);
                MessageFactory.CacheMessage(applyCombatStatsMsg);
            }
            base.Destroy();
        }
    }
}