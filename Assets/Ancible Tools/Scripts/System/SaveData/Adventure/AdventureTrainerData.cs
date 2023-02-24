using System;
using Assets.Ancible_Tools.Scripts.System.SaveData.Adventure;

namespace Assets.Resources.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class AdventureTrainerData : AdventureParameterData
    {
        public const string FILTER = "AdventureTrainer-";

        public override string GenerateFilter()
        {
            return $"{FILTER}{Id}";
        }
    }
}