using System;
using Assets.Ancible_Tools.Scripts.System.BattleLeague.Conditions;
using Assets.Ancible_Tools.Scripts.Traits;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Abilities
{
    [CreateAssetMenu(fileName = "World Ability", menuName = "Ancible Tools/Abilities/World Ability")]
    public class WorldAbility : ScriptableObject
    {
        public string DisplayName;
        public Sprite Icon;
        public int Rank = 0;
        [TextArea(1, 5)] public string Description;
        public int Range = 1;
        public int CastTime = 1;
        public int ManaCost = 0;
        public Trait[] ApplyToTarget;
        public AbilityTargetAlignment Alignment;
        public AbilityType Type;
        public int Cooldown = 1;
        public BattleCondition[] Conditions = new BattleCondition[0];
        

        public void UseAbility(GameObject owner, GameObject target)
        {
            var obj = Type == AbilityType.Self ? owner : target;
            var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
            for (var i = 0; i < ApplyToTarget.Length; i++)
            {
                addTraitToUnitMsg.Trait = ApplyToTarget[i];
                owner.SendMessageTo(addTraitToUnitMsg, obj);
            }
            MessageFactory.CacheMessage(addTraitToUnitMsg);
        }

        public AbilityInstance GenerateInstance()
        {
            return new AbilityInstance(this);
        }

        public bool PassesConditions(GameObject owner, GameObject target)
        {
            if (Conditions.Length > 0)
            {
                for (var i = 0; i < Conditions.Length; i++)
                {
                    if (Conditions[i].PassesCondition(owner, target))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public string GetDescription()
        {
            var description = $"{(Rank > 0 ? StaticMethods.ApplyColorToText($"Rank {this.RankToString()}{Environment.NewLine}", ColorFactoryController.AbilityRank) : string.Empty)}Range: {Range}";
            description = $"{description}{Environment.NewLine}Cast Time: {(CastTime * TickController.TickRate):F1}s";
            if (ManaCost > 0)
            {
                description = $"{description}{Environment.NewLine}{ManaCost} Mana";
            }

            switch (Alignment)
            {
                case AbilityTargetAlignment.Ally:
                    description = $"{description}{Environment.NewLine}Ally Only";
                    break;
                case AbilityTargetAlignment.Enemy:
                    description = $"{description}{Environment.NewLine}Enemy Only";
                    break;
                case AbilityTargetAlignment.Both:
                    break;
            }
            var traitDescriptions = ApplyToTarget.GetTraitDescriptions();
            
            if (traitDescriptions.Length > 0)
            {
                var initialDescription = $"Applies to {(Type == AbilityType.Self ? "Self" : "Target")}:{Environment.NewLine}";
                description = $"{description}{Environment.NewLine}{Environment.NewLine}{initialDescription}";
                for (var i = 0; i < traitDescriptions.Length; i++)
                {
                    if (i == 0)
                    {
                        description = $"{description}{traitDescriptions[i]}";
                        if (i + 1 < traitDescriptions.Length - 1)
                        {
                            description = $"{description},";
                        }
                    }
                    else if (i < traitDescriptions.Length - 1)
                    {
                        
                        description = $"{description} {traitDescriptions[i]}";
                        if (i + 1 < traitDescriptions.Length - 1)
                        {
                            description = $"{description},";
                        }
                    }
                    else
                    {
                        description = i == 0 ? $"{description} {traitDescriptions[i]}" : $"{description} and {traitDescriptions[i]}";
                    }
                }
            }

            if (!string.IsNullOrEmpty(Description))
            {
                description = $"{description}{Environment.NewLine}{Environment.NewLine}{Description}";
            }

            return description;
        }

    }
}