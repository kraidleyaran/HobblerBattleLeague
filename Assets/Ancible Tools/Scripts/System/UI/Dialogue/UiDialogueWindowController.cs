using System;
using System.Linq;
using Assets.Ancible_Tools.Scripts.System.Dialogue;
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
        private Action _doAfter = null;

        public override void Awake()
        {
            base.Awake();
            _answerManager.Clear();
            SubscribeToMessages();
        }

        public void Setup(DialogueData dialogue, GameObject owner, Action doAfter = null)
        {
            _currentDialogue = dialogue;
            _owner = owner;
            _answerManager.Clear();
            _doAfter = doAfter;
            _dialogueTextController.Setup(_currentDialogue.Dialogue);
        }

        public void Setup(string[] dialogue, GameObject owner, Action doAfter = null)
        {
            _owner = owner;
            _answerManager.Clear();
            _doAfter = doAfter;
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
                if (!_dialogueTextController.SkipText())
                {
                    if (_currentDialogue && (_currentDialogue.ApplyToOwner.Length > 0 || _currentDialogue.ApplyToPlayer.Length > 0))
                    {
                        var addTraitToUnitMsg = MessageFactory.GenerateAddTraitToUnitMsg();
                        foreach (var trait in _currentDialogue.ApplyToOwner)
                        {
                            addTraitToUnitMsg.Trait = trait;
                            gameObject.SendMessageTo(addTraitToUnitMsg, _owner);
                        }

                        foreach (var trait in _currentDialogue.ApplyToPlayer)
                        {
                            addTraitToUnitMsg.Trait = trait;
                            gameObject.SendMessageTo(addTraitToUnitMsg, WorldAdventureController.Player);
                        }
                        MessageFactory.CacheMessage(addTraitToUnitMsg);
                    }
                    if ((!_currentDialogue || _currentDialogue.Tree.Dialogue.Length <= 0))
                    {
                        Close();
                    }
                    
                }
            }
        }

        public override void Close()
        {
            _doAfter?.Invoke();
            _doAfter = null;
            gameObject.SendMessageTo(DialogueClosedMessage.INSTANCE, _owner);
            base.Close();
        }
    }
}