using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameStatusBarManager : UiBaseWindow
    {
        private static UiMinigameStatusBarManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private UiMinigameUnitStatusBarController _statusBarTemplate = null;

        private Dictionary<GameObject, UiMinigameUnitStatusBarController> _statusBars = new Dictionary<GameObject, UiMinigameUnitStatusBarController>();

        public override void Awake()
        {
            base.Awake();
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
        }

        public static void RegisterMinigameUnit(GameObject unit, Transform follow, Color healthBarColor, Vector2 offset)
        {
            if (_instance)
            {
                if (!_instance._statusBars.ContainsKey(unit))
                {
                    var controller = Instantiate(_instance._statusBarTemplate, _instance.transform);
                    controller.Setup(unit, follow, healthBarColor, offset);
                    _instance._statusBars.Add(unit, controller);
                }
            }

        }

        public static void RemoveMinigameUnit(GameObject unit)
        {
            if (_instance)
            {
                if (_instance._statusBars.TryGetValue(unit, out var statusBar))
                {
                    statusBar.Destroy();
                    Destroy(statusBar.gameObject);
                    _instance._statusBars.Remove(unit);
                }
            }
        }

        public override void Destroy()
        {
            if (_instance && _instance == this)
            {
                _instance = null;
            }
            base.Destroy();
        }
    }
}