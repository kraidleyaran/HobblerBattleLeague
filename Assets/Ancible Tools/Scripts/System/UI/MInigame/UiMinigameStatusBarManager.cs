using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.MInigame
{
    public class UiMinigameStatusBarManager : UiBaseWindow
    {
        public static UiFloatingTextController FloatingText => _instance._floatingTextTemplate;

        private static UiMinigameStatusBarManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private UiMinigameUnitStatusBarController _statusBarTemplate = null;
        [SerializeField] private UiFloatingTextController _floatingTextTemplate = null;



        private Dictionary<GameObject, UiMinigameUnitStatusBarController> _statusBars = new Dictionary<GameObject, UiMinigameUnitStatusBarController>();
        private List<UiFloatingTextController> _floatingText = new List<UiFloatingTextController>();
        private Dictionary<UiFloatingTextController, GameObject> _placeHolders = new Dictionary<UiFloatingTextController, GameObject>();


        public override void Awake()
        {
            base.Awake();
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            SubscribeToMessages();
        }

        private void FloatingTextFinished(UiFloatingTextController controller)
        {
            _floatingText.Remove(controller);
            Destroy(_placeHolders[controller]);
            Destroy(controller.gameObject);
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

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<ShowFloatingTextMessage>(ShowFloatingText);
        }

        private void ShowFloatingText(ShowFloatingTextMessage msg)
        {
            var controller = Instantiate(FloatingText, transform);
            var right = _floatingText.Count > 0 && !_floatingText[_floatingText.Count - 1];
            _floatingText.Add(controller);
            var placeHolder = Instantiate(FactoryController.INVISIBLE, msg.World, Quaternion.identity);
            _placeHolders.Add(controller, placeHolder);
            controller.Setup(StaticMethods.ApplyColorToText(msg.Text, msg.Color), right, FloatingTextFinished, placeHolder);
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