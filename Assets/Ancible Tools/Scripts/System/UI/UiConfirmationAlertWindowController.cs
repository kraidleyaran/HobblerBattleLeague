using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiConfirmationAlertWindowController : UiBaseWindow
    {
        public override bool Static => true;
        public override bool Movable => false;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Text _alertText;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private float _baseHeight = 200f;
        [SerializeField] private float _minimumHeight = 200f;


        private Action _onConfirm = null;

        public void Setup(string alert, Sprite icon, Action onConfirm, Color colorMask)
        {
            _alertText.text = alert;
            _onConfirm = onConfirm;
            _iconImage.sprite = icon;
            var textHeight = _alertText.GetHeightOfText(alert);
            _alertText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
            var height = Mathf.Max(textHeight + _baseHeight, _minimumHeight);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public void Confirm()
        {
            _onConfirm?.Invoke();
            Close();
        }
    }
}