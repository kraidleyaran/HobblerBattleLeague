using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Alerts
{
    public class UiAlertController : MonoBehaviour
    {
        [SerializeField] private RectTransform _gridTransform;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Image _iconBorder = null;
        [SerializeField] private Text _alertText = null;
        [SerializeField] private int _moveTime = 0;
        [SerializeField] private float _movePosition = 0f;
        [SerializeField] private int _stayTime = 0;
        [SerializeField] private Ease _ease = Ease.Linear;

        private Sequence _staySequence = null;
        private Tween _moveTween = null;

        public void Setup(string text, Sprite icon, Color iconBorderColor)
        {
            _alertText.text = text;
            _iconImage.sprite = icon;
            _moveTween = _gridTransform.DOLocalMoveX(_movePosition, _moveTime * TickController.TickRate).SetEase(_ease).OnComplete(MoveFinished);
            _iconBorder.color = iconBorderColor;
        }

        private void MoveFinished()
        {
            _moveTween = null;
            _staySequence = DOTween.Sequence().AppendInterval(_stayTime * TickController.TickRate).OnComplete(StayFinished);
        }

        private void StayFinished()
        {
            _staySequence = null;
            UiAlertManager.RemoveAlert(this);
        }

        public void Click()
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }
            else if (_staySequence != null)
            {
                if (_staySequence.IsActive())
                {
                    _staySequence.Kill();
                }

                _staySequence = null;
            }
            UiAlertManager.RemoveAlert(this);
        }

        void OnDestroy()
        {
            if (_moveTween != null)
            {
                if (_moveTween.IsActive())
                {
                    _moveTween.Kill();
                }

                _moveTween = null;
            }
            else if (_staySequence != null)
            {
                if (_staySequence.IsActive())
                {
                    _staySequence.Kill();
                }

                _staySequence = null;
            }
        }
    }
}