using DG.Tweening;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Dialogue
{
    public class UiDialogueTextController : MonoBehaviour
    {
        [SerializeField] private Text _dialogueText = null;
        [SerializeField] private int _textSpeed = 0;

        private Tween _dialogueTween = null;

        private int _index = 0;
        private string[] _dialogue = new string[0];

        void Awake()
        {
            _dialogueText.text = string.Empty;
        }

        public void Setup(string[] dialogue)
        {
            _index = 0;
            _dialogue = dialogue;
            ShowText(_dialogue[_index]);
        }

        public bool SkipText()
        {
            if (_dialogueTween != null)
            {
                _dialogueTween.Complete();
                return true;
            }

            if (_index < _dialogue.Length - 1)
            {
                _index++;
                ShowText(_dialogue[_index]);
                return true;
            }

            return false;

        }

        private void ShowText(string text)
        {
            _dialogueText.text = string.Empty;
            var formattedText = _dialogueText.GetFormmatedTextLines(text).ToSingleString();
            var timeToType = formattedText.Length * (TickController.OneSecond / _textSpeed);
            _dialogueTween = _dialogueText.DOText(formattedText, timeToType).SetEase(Ease.Linear).OnComplete(() =>
            {
                _dialogueTween = null;
                if (_index == _dialogue.Length - 1)
                {
                    gameObject.SendMessage(ShowCurrentDialogueAnswersMessage.INSTANCE);
                }
            });
        }
    }
}