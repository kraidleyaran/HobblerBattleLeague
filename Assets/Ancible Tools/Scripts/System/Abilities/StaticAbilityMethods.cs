using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Combat;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    public static class StaticAbilityMethods
    {
        public static bool CanApplyToTarget(this WorldAbility ability, GameObject owner, GameObject target, int distance, bool battle = true)
        {
            if (!ability.PassesConditions(owner, target))
            {
                return false;
            }
            if (ability.Type == AbilityType.Passive)
            {
                return false;
            }
            if (ability.Type == AbilityType.Self)
            {
                return true;
            }

            if (ability.Type == AbilityType.Other && owner == target)
            {
                return false;
            }

            if (ability.Range < distance)
            {
                return false;
            }
            if (battle)
            {
                var ownerAlignment = BattleAlignment.None;
                var targetAlignment = BattleAlignment.None;

                var queryBattleAlignmentMsg = MessageFactory.GenerateQueryBattleAlignmentMsg();
                queryBattleAlignmentMsg.DoAfter = alignment => ownerAlignment = alignment;
                owner.SendMessageTo(queryBattleAlignmentMsg, owner);

                queryBattleAlignmentMsg.DoAfter = alignment => targetAlignment = alignment;
                owner.SendMessageTo(queryBattleAlignmentMsg, target);

                switch (ability.Alignment)
                {
                    case AbilityTargetAlignment.Ally:
                        return ownerAlignment == targetAlignment;
                    case AbilityTargetAlignment.Enemy:
                        return ownerAlignment != targetAlignment;
                    default:
                        return true;
                }
            }
            else
            {
                var ownerAlignment = CombatAlignment.Neutral;
                var targetAlignment = CombatAlignment.Neutral;

                var queryCombatAlignmentMsg = MessageFactory.GenerateQueryCombatAlignmentMsg();
                queryCombatAlignmentMsg.DoAfter = alignment => ownerAlignment = alignment;
                owner.SendMessageTo(queryCombatAlignmentMsg, owner);

                queryCombatAlignmentMsg.DoAfter = alignment => targetAlignment = alignment;
                owner.SendMessageTo(queryCombatAlignmentMsg, target);

                switch (ability.Alignment)
                {
                    case AbilityTargetAlignment.Ally:
                        return ownerAlignment == targetAlignment;
                    case AbilityTargetAlignment.Enemy:
                        return ownerAlignment != targetAlignment;
                    default:
                        return true;
                }
            }
        }
    }
}