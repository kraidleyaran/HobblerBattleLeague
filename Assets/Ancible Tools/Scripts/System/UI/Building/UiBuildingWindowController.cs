using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.Building;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Building
{
    public class UiBuildingWindowController : UiBaseWindow
    {
        public override bool Movable => true;
        public override bool Static => true;

        [SerializeField] private UiBuildingButtonController _buttonTemplate = null;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private RectTransform _content;

        private UiBuildingButtonController[] _buttons = new UiBuildingButtonController[0];

        public override void Awake()
        {
            base.Awake();
            var buildings = WorldBuildingManager.Available.ToArray();
            var buttons = new List<UiBuildingButtonController>();
            for (var i = 0; i < buildings.Length; i++)
            {
                var button = Instantiate(_buttonTemplate, _grid.transform);
                button.Setup(buildings[i]);
                buttons.Add(button);
            }

            _buttons = buttons.ToArray();
            var rows = buttons.Count / _grid.constraintCount;
            var rowCheck = rows * _grid.constraintCount;
            if (rowCheck < _buttons.Length)
            {
                rows++;
            }

            var height = rows * (_grid.spacing.y + _grid.cellSize.y) + _grid.padding.top;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override void Destroy()
        {
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Destroy();
            }
            base.Destroy();
        }
    }


}