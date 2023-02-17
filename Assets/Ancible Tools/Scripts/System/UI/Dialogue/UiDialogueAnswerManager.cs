using System.Collections.Generic;
using Assets.Ancible_Tools.Scripts.System.Dialogue;
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Dialogue
{
    public class UiDialogueAnswerManager : MonoBehaviour
    {
        [SerializeField] private UiDialogueAnswerController _answerTemplate;
        [SerializeField] private VerticalLayoutGroup _grid;
        [SerializeField] private RectTransform _content = null;
        [SerializeField] private float _windowBuffer = 20f;
        [SerializeField] private float _maxHeight = 360f;
        [SerializeField] private RectTransform _rectTransform = null;

        private UiDialogueAnswerController[] _controllers = new UiDialogueAnswerController[0];

        public void Setup(DialogueTree tree, GameObject owner)
        {
            foreach (var controller in _controllers)
            {
                Destroy(controller.gameObject);
            }

            var controllers = new List<UiDialogueAnswerController>();
            gameObject.SetActive(true);
            foreach (var dialogue in tree.Dialogue)
            {
                var controller = Instantiate(_answerTemplate, _grid.transform);
                controller.Setup(dialogue, owner);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();

            var height = _controllers.Length * (_grid.spacing + _answerTemplate.RectTransform.rect.height) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            var rectHeight = Mathf.Min(height + _windowBuffer, _maxHeight);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectHeight);


        }

        public void Clear()
        {
            foreach (var controller in _controllers)
            {
                Destroy(controller.gameObject);
            }

            _controllers = new UiDialogueAnswerController[0];
            gameObject.SetActive(false);
        }
        
    }
}