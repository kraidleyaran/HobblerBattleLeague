using Assets.Resources.Ancible_Tools.Scripts.System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.BattleLeague.Conditions
{
    [CreateAssetMenu(fileName = "Casting Battle Condition", menuName = "Ancible Tools/Battle/Conditions/Casting")]
    public class CastingBattleCondition : BattleCondition
    {
        public override bool PassesCondition(GameObject owner, GameObject target)
        {
            var pass = false;
            var queryCastingMsg = MessageFactory.GenerateQueryCastingMsg();
            queryCastingMsg.DoAfter = () => { pass = true;};
            owner.SendMessageTo(queryCastingMsg, target);
            MessageFactory.CacheMessage(queryCastingMsg);

            return pass;
        }
    }
}