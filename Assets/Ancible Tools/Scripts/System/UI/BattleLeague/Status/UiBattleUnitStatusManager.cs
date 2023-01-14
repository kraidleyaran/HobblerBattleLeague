using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status
{
    public class UiBattleUnitStatusManager : UiBaseWindow
    {
        public static UiBattleUnitStatusManager _instance = null;

        [SerializeField] private Color _leftSideHealthColor = Color.blue;
        [SerializeField] private Color _rightSideHealthColor = Color.red;
        [SerializeField] private UiBattleUnitStatusController _unitStatusTemplate = null;

        public override bool Static => true;
        public override bool Movable => false;

        private Dictionary<GameObject, UiBattleUnitStatusController> _controllers = new Dictionary<GameObject, UiBattleUnitStatusController>();

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            base.Awake();
        }

        public static void RegisterUnitStatusBar(GameObject unit, Transform follow, BattleAlignment alignment, Vector2 offset)
        {
            if (!_instance._controllers.ContainsKey(unit))
            {
                var controller = Instantiate(_instance._unitStatusTemplate, _instance.transform);
                var color = alignment == BattleAlignment.Left ? _instance._leftSideHealthColor : _instance._rightSideHealthColor;
                controller.Setup(unit, follow, color, offset);
                _instance._controllers.Add(unit, controller);
            }
        }

        public static void Clear()
        {
            var controllers = _instance._controllers.Values.ToArray();
            for (var i = 0; i < controllers.Length; i++)
            {
                controllers[i].Destroy();
                Destroy(controllers[i].gameObject);
            }
            _instance._controllers.Clear();
        }
    }
}