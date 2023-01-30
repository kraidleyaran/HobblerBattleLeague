using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Ancible_Tools.Scripts.System.BattleLeague;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.BattleLeague.Status
{
    public class UiBattleUnitStatusManager : UiBaseWindow
    {
        public static UiFloatingTextController FloatingText => _instance._floatingTextTemplate;
        public static UiBattleUnitStatusManager _instance = null;
        

        [SerializeField] private Color _leftSideHealthColor = Color.blue;
        [SerializeField] private Color _rightSideHealthColor = Color.red;
        [SerializeField] private UiBattleUnitStatusController _unitStatusTemplate = null;
        [SerializeField] private UiFloatingTextController _floatingTextTemplate = null;

        public override bool Static => true;
        public override bool Movable => false;

        private Dictionary<GameObject, UiBattleUnitStatusController> _controllers = new Dictionary<GameObject, UiBattleUnitStatusController>();
        private List<UiFloatingTextController> _floatingText = new List<UiFloatingTextController>();
        private Dictionary<UiFloatingTextController, GameObject> _placeHolders = new Dictionary<UiFloatingTextController, GameObject>();

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(_instance.gameObject);
            }

            _instance = this;
            base.Awake();
            SubscribeToMessages();
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

        private void FloatingTextFinished(UiFloatingTextController controller)
        {
            _floatingText.Remove(controller);
            Destroy(_placeHolders[controller]);
            Destroy(controller.gameObject);
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


    }
}