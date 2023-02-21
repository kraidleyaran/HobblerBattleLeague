using System.Collections.Generic;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Dialogue
{
    public class DialogueFactory : MonoBehaviour
    {
        public static string[] MonsterUnpreparedBattle => _instance._monsterUnpreparedBattle;
        public static string[] TrainerUnpreparedBattle => _instance._trainerUnpreparedBattle;
        public static string[] DefaultDefeat => _instance._defaultDefeat;

        private static DialogueFactory _instance = null;

        private Dictionary<string, DialogueData> _dialogue = new Dictionary<string, DialogueData>();

        [SerializeField] [TextArea(3,5)] private string[] _monsterUnpreparedBattle = new string[0];
        [SerializeField] [TextArea(3,5)] private string[] _trainerUnpreparedBattle = new string[0];
        [SerializeField] [TextArea(3,5)] private string[] _defaultDefeat = new string[0];
        [SerializeField] private string _dialogueFolder = string.Empty;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            var dialogues = UnityEngine.Resources.LoadAll<DialogueData>(_dialogueFolder);
            foreach (var dialogue in dialogues)
            {
                if (!_dialogue.ContainsKey(dialogue.name))
                {
                    _dialogue.Add(dialogue.name, dialogue);
                }
                else
                {
                    Debug.LogWarning($"Duplicate Dialogue Name: {dialogue.name}");
                }
            }
            _instance = this;
        }

        public static DialogueData GetDialogueFromName(string name)
        {
            return _instance._dialogue.TryGetValue(name, out var dialogue) ? dialogue : null;
        }
    }
}