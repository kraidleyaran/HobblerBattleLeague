using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    [CreateAssetMenu(fileName = "Dialogue Data", menuName = "Ancible Tools/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public string Subject = string.Empty;
        [TextArea(3,5)] public string[] Dialogue = new string[0];
        public DialogueTree Tree = new DialogueTree();
    }
}