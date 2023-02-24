using System;
using Assets.Ancible_Tools.Scripts.System.SaveData.Adventure;

namespace Assets.Ancible_Tools.Scripts.System.SaveData
{
    [Serializable]
    public class AdventureDialogueData : AdventureParameterData
    {
        public const string FILTER = "AdventureDialogue-";

        public string Dialogue;

        public override string GenerateFilter()
        {
            return $"{FILTER}{Id}";
        }
    }
}