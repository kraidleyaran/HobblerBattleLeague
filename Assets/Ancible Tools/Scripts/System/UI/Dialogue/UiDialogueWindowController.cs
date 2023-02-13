using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Dialogue
{
    public class UiDialogueWindowController : UiBaseWindow
    {
        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private UiDialogueTextController _dialogueTextController;
        [SerializeField] private UiDialogueAnswerManager _answerManager;

        private DialogueData _currentDialogue = null;
        private GameObject _owner = null;

        public override void Awake()
        {
            base.Awake();
            _answerManager.Clear();
            SubscribeToMessages();
        }

        public void Setup(DialogueData dialogue, GameObject owner)
        {
            _currentDialogue = dialogue;
            _owner = owner;
            _answerManager.Clear();
            _dialogueTextController.Setup(_currentDialogue.Dialogue);
        }

        public void Setup(string[] dialogue, GameObject owner)
        {
            _owner = owner;
            _answerManager.Clear();
            _dialogueTextController.Setup(dialogue);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowCurrentDialogueAnswersMessage>(ShowCurrentDialogueAnswers);
            gameObject.Subscribe<UpdateInputStateMessage>(UpdateInputState);
        }

        private void ShowCurrentDialogueAnswers(ShowCurrentDialogueAnswersMessage msg)
        {
            if (_currentDialogue)
            {
                if (_currentDialogue.Tree.Dialogue.Length > 0 && !_answerManager.gameObject.activeSelf)
                {
                    _answerManager.Setup(_currentDialogue.Tree, _owner);
                }   
            }
        }

        private void UpdateInputState(UpdateInputStateMessage msg)
        {
            if (!msg.Previous.Interact && msg.Current.Interact && !_answerManager.gameObject.activeSelf)
            {
                if (!_dialogueTextController.SkipText() &&  (!_currentDialogue || _currentDialogue.Tree.Dialogue.Length <= 0))
                {
                    Close();
                }
            }
        }

        public override void Close()
        {
            gameObject.SendMessageTo(DialogueClosedMessage.INSTANCE, _owner);
            base.Close();
        }
    }
}