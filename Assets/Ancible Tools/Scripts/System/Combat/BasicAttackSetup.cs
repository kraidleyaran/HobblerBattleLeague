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

        public string GetDescription()
        {
            var description = $"{AttackSpeed} Attack Speed{Environment.NewLine}{Range} Range";
            var traitDescriptions = ApplyToTarget.OrderByDescending(t => t.DescriptionPriority).Select(t => t.GetDescription()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
            if (traitDescriptions.Length > 0)
            {
                for (var i = 0; i < traitDescriptions.Length; i++)
                {
                    description = $"{description}{Environment.NewLine}{traitDescriptions[i]}";
                }
            }
            return description;

        }
    }
}