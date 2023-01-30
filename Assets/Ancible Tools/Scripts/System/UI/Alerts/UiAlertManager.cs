using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;
using static UnityEngine.Object;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts
{
    public class UiAlertManager : UiBaseWindow
    {
        private static UiAlertManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private UiAlertController _alertTemplate;
        [SerializeField] private RectTransform _gridTransform;
        [SerializeField] private int _alertCooldownTime = 5;

        private List<UiAlertController> _controllers = new List<UiAlertController>();
        private List<CachedAlert> _alerts = new List<CachedAlert>();
        private TickTimer _alertCooldownTimer = null;
        private bool _onCooldown = false;

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _alertCooldownTimer = new TickTimer(_alertCooldownTime, 0, AlertCooldownFinished, null, false, false);
            base.Awake();
        }

        private void AlertCooldownFinished()
        {
            _onCooldown = false;
            if (_alerts.Count > 0)
            {
                var alert = _alerts[0];
                _alerts.RemoveAt(0);
                GenerateAlert(alert.Text, alert.Icon);
                alert.Dispose();
            }
        }

        private void GenerateAlert(string text, Sprite icon)
        {
            var controller = Instantiate(_alertTemplate, _gridTransform);
            controller.Setup(text, icon);
            _alertCooldownTimer.Play();
        }

        public static void ShowAlert(string text, Sprite icon)
        {
            if (_instance._onCooldown)
            {
                _instance._alerts.Add(new CachedAlert(text, icon));
            }
            else
            {
                _instance.GenerateAlert(text, icon);
            }
        }

        public static void RemoveAlert(UiAlertController controller)
        {
            _instance._controllers.Remove(controller);
            Destroy(controller.gameObject);
        }
    }
}