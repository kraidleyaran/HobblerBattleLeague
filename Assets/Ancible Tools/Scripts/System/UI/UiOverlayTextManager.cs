using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiOverlayTextManager : UiBaseWindow
    {
        private static UiOverlayTextManager _instance = null;

        public override bool Movable => false;
        public override bool Static => true;

        [SerializeField] private Text _permanentText = null;
        [SerializeField] private Text _overlayAlertText = null;
        [SerializeField] public CanvasGroup _overlayAlertTextCanvasGroup = null;
        [SerializeField] private int _overlayAliveTime = 120;
        [SerializeField] private int _overlayFadeTime = 30;
        [SerializeField] private Ease _fadingEase = Ease.Linear;

        private TickTimer _overlayTextTimer = null;
        private Tween _overlayFadeTween = null;

        public override void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _overlayTextTimer = new TickTimer(_overlayAliveTime, 0, OverlayAlertTextTimerFinished, null, false);
            _overlayAlertText.text = string.Empty;
            _instance = this;
            base.Awake();
        }

        private void OverlayAlertTextTimerFinished()
        {
            if (_overlayFadeTween != null)
            {
                if (_overlayFadeTween.IsActive())
                {
                    _overlayFadeTween.Kill();
                }

                _overlayFadeTween = null;
            }

            _overlayFadeTween = _overlayAlertTextCanvasGroup.DOFade(0f, _overlayFadeTime * TickController.TickRate).SetEase(_fadingEase).OnComplete(OverlayAlertFadeFinished);
        }

        private void OverlayAlertFadeFinished()
        {
            _overlayFadeTween = null;
            _overlayAlertText.text = string.Empty;
            _overlayAlertTextCanvasGroup.alpha = 1f;

        }

        public static void ShowOverlayAlert(string text, Color color)
        {
            if (_instance._overlayFadeTween != null)
            {
                if (_instance._overlayFadeTween.IsActive())
                {
                    _instance._overlayFadeTween.Kill();
                }

                _instance._overlayFadeTween = null;
            }

            _instance._overlayTextTimer.Restart();
            if (_instance._overlayTextTimer.State != TimerState.Playing)
            {
                _instance._overlayTextTimer.Play();
            }

            _instance._overlayAlertText.text = $"{StaticMethods.ApplyColorToText(text, color)}";
            _instance._overlayAlertTextCanvasGroup.alpha = 1f;
        }

        public override void Destroy()
        {
            if (_overlayFadeTween != null)
            {
                if (_overlayFadeTween.IsActive())
                {
                    _overlayFadeTween.Kill();
                }

                _overlayFadeTween = null;
            }

            _overlayTextTimer.Destroy();
            _overlayTextTimer = null;
            base.Destroy();
        }
    }
}