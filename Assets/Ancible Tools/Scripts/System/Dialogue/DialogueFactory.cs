using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Dialogue
{
    public class DialogueFactory : MonoBehaviour
    {
        public static string[] MonsterUnpreparedBattle => _instance._monsterUnpreparedBattle;
        public static string[] TrainerUnpreparedBattle => _instance._trainerUnpreparedBattle;

        private static DialogueFactory _instance = null;

        [SerializeField] [TextArea(3,5)] private string[] _monsterUnpreparedBattle = new string[0];
        [SerializeField] [TextArea(3,5)] private string[] _trainerUnpreparedBattle = new string[0];

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }
    }
}