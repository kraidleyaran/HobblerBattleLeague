using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
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
            _alertCooldownTimer = new TickTimer(_alertCooldownTime, 0, AlertCooldownFinished, null, false);

            base.Awake();
        }

        private void AlertCooldownFinished()
        {
            _onCooldown = false;
            if (_alerts.Count > 0)
            {
                var alert = _alerts[0];
                _alerts.RemoveAt(0);
                GenerateAlert(alert.Text, alert.Icon, alert.BorderColor);
                alert.Dispose();
            }
        }

        private void GenerateAlert(string text, Sprite icon,Color borderColor)
        {
            var controller = Instantiate(_alertTemplate, _gridTransform);
            controller.Setup(text, icon, borderColor);
            _alertCooldownTimer.Play();
        }

        public static void ShowAlert(string text, Sprite icon, Color borderColor)
        {
            if (_instance._onCooldown)
            {
                _instance._alerts.Add(new CachedAlert(text, icon, borderColor));
            }
            else
            {
                _instance.GenerateAlert(text, icon, borderColor);
            }
        }

        public static void RemoveAlert(UiAlertController controller)
        {
            _instance._controllers.Remove(controller);
            Destroy(controller.gameObject);
        }

        public override void Destroy()
        {

            _alertCooldownTimer.Destroy();
            _alertCooldownTimer = null;
            base.Destroy();
        }
    }
}