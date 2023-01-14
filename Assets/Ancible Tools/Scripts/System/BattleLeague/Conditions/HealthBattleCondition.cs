using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Abilities;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.BattleLeague.Conditions
{
    [CreateAssetMenu(fileName = "Health Battle Condition", menuName = "Ancible Tools/Battle/Conditions/Health")]
    public class HealthBattleCondition : BattleCondition
    {
        [SerializeField] [Range(0f, 1f)] private float _healthPercent = 0f;
        [SerializeField] private ComparisonType _comparison = ComparisonType.Equal;

        public override bool PassesCondition(GameObject owner, GameObject target)
        {
            var targetHealthPercent = 0f;
            var queryHealthMsg = MessageFactory.GenerateQueryHealthMsg();
            queryHealthMsg.DoAfter = (current, max) => targetHealthPercent = (float) current / max;
            owner.SendMessageTo(queryHealthMsg, target);
            MessageFactory.CacheMessage(queryHealthMsg);

            return targetHealthPercent.EqualityCompare(_comparison, _healthPercent);
        }
    }
}