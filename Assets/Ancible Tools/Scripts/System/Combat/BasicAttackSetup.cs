using System;
using System.Linq;
using Assets.Ancible_Tools.Scripts.Traits;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Combat
{
    [Serializable]
    public class BasicAttackSetup
    {
        public int Range = 0;
        public int AttackSpeed = 0;
        public Trait[] ApplyToTarget = new Trait[0];
        public Trait[] ApplyToOwner = new Trait[0];

        public string GetDescription()
        {
            var description = $"{Range} Range{Environment.NewLine}{AttackSpeed} Attack Speed{Environment.NewLine}";
            var traitDescriptions = ApplyToTarget.OrderByDescending(t => t.DescriptionPriority).Select(t => t.GetDescription()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (traitDescriptions.Length > 0)
            {
                for (var i = 0; i < traitDescriptions.Length; i++)
                {
                    description = $"{description}{Environment.NewLine}{traitDescriptions[i]}";
                }
            }

            var ownerDescriptions = ApplyToOwner.OrderByDescending(t => t.DescriptionPriority).Select(t=> t.GetDescription()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (ownerDescriptions.Length > 0)
            {
                description = $"{description}{StaticMethods.DoubleNewLine()}{StaticMethods.ApplyColorToText("On Use:", ColorFactoryController.BonusStat)}";
                for (var i = 0; i < ownerDescriptions.Length; i++)
                {
                    if (i == 0)
                    {
                        description = $"{description} {ownerDescriptions[i]}";
                    }
                    else if (i < ownerDescriptions.Length - 1)
                    {
                        description = $"{description}, {ownerDescriptions[i]}";
                    }
                    else
                    {
                        description = $"{description} and {ownerDescriptions[i]}";
                    }
                    
                }
            }
            return description;

        }
    }
}