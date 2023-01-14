using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Dialogue
{
    public class UiDialogueAnswerController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform RectTransform;

        [SerializeField] private Text _subjectText = null;
        [SerializeField] private Image _frameImage = null;

        public DialogueData Dialogue { get; private set; }

        private GameObject _owner = null;

        public void Setup(DialogueData dialogue, GameObject owner)
        {
            Dialogue = dialogue;
            _owner = owner;
            _subjectText.text = dialogue.Subject;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _frameImage.color = ColorFactoryController.HoveredItem;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _frameImage.color = Color.white;
        }

        public void ClickDialogue()
        {
            var showDialogueMsg = MessageFactory.GenerateShowDialogueMsg();
            showDialogueMsg.Dialogue = Dialogue;
            showDialogueMsg.Owner = _owner;
            gameObject.SendMessage(showDialogueMsg);
            MessageFactory.CacheMessage(showDialogueMsg);
        }
    }
}