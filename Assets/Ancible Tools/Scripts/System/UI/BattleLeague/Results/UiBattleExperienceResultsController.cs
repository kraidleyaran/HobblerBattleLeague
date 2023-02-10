using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleExperienceResultsController : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _grid = null;
        [SerializeField] private RectTransform _content = null;
        [SerializeField] private UiBattleUnitExperienceController _battleUnitExperienceTemplate;
        [SerializeField] private Text _totalExperienceGainedText = null;
        [SerializeField] private int _waitFrames = 15;

        private UiBattleUnitExperienceController[] _controllers = new UiBattleUnitExperienceController[0];
        private Coroutine _waitCoroutine = null;

        public void Setup(GameObject[] hobblers, int experienceGained)
        {
            var controllers = new List<UiBattleUnitExperienceController>();
            foreach (var hob in hobblers)
            {
                var controller = Instantiate(_battleUnitExperienceTemplate, _grid.transform);
                controller.Setup(hob, experienceGained);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            var totalRows = _grid.constraintCount / _controllers.Length;
            var rowCheck = totalRows * _grid.constraintCount;
            if (rowCheck < _controllers.Length)
            {
                totalRows++;
            }

            var height = totalRows * (_grid.cellSize.y + _grid.spacing.y) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            _totalExperienceGainedText.text = $"{experienceGained:n0}";
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            _waitCoroutine = StartCoroutine(StaticMethods.WaitForFrames(_waitFrames, ActivateControllers));

        }

        private void ActivateControllers()
        {
            _waitCoroutine = null;
            foreach (var controller in _controllers)
            {
                controller.ActivateBar();
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }


}