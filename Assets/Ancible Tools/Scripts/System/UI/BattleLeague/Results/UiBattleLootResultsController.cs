using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.UI.Stash;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague
{
    public class UiBattleLootResultsController : MonoBehaviour
    {
        [SerializeField] private UiStashItemController _stashItemTemplate;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private Text _noLootText = null;
        [SerializeField] private int _maxRows = 4;

        private UiStashItemController[] _controllers = new UiStashItemController[0];

        public void Setup(ItemStack[] items)
        {
            _noLootText.gameObject.SetActive(items.Length <= 0);
            var controllers = new List<UiStashItemController>();
            foreach (var item in items)
            {
                var controller = Instantiate(_stashItemTemplate, _grid.transform);
                controller.Setup(item, false);
                controllers.Add(controller);
            }

            _controllers = controllers.ToArray();
            var totalRows = _controllers.Length / _grid.constraintCount;
            if (totalRows < _maxRows)
            {
                totalRows = _maxRows;
            }
            //var rowCheck = totalRows * _grid.constraintCount;
            //if (rowCheck < _controllers.Length)
            //{
            //    totalRows++;
            //}

            var height = totalRows * (_grid.cellSize.y + _grid.spacing.y) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}