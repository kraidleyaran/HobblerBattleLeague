using System;
using Assets.Resources.Ancible_Tools.Scripts.System.Minigame;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI
{
    public class UiFloatingTextController : MonoBehaviour
    {
        public bool IsRight { get; private set; }

        [SerializeField] private Text _text;
        [SerializeField] private float _travelTime;
        [SerializeField] private Vector2 _localTravelPosition = Vector2.up;
        [SerializeField] private float _jumpPower = 0f;
        [SerializeField] private float _stayTime = 0f;
        [SerializeField] private Ease _ease = Ease.Linear;

        private Action<UiFloatingTextController> _onFinish = null;
        private Tween _moveTween = null;
        private GameObject _parent = null;

        public void Setup(string text, bool right, Action<UiFloatingTextController> onFinish, GameObject parent)
        {
            _parent = parent;
            var pos = _localTravelPosition;
            if (right)
            {
                pos.x *= -1;
            }

            IsRight = right;
            _onFinish = onFinish;
            _text.text = text;
            _moveTween = _text.transform.DOLocalJump(pos, _jumpPower, 1, _travelTime).SetEase(_ease).OnComplete(() =>
            {
                _moveTween = null;
                _onFinish?.Invoke(this);
                Destroy(_parent);
            });
        }

        void Update()
        {
            if (_parent)
            {
                
                var pos = WorldController.GetCurrentCamera().WorldToScreenPoint(_parent.transform.position.ToVector2()).ToVector2();
                if (pos != transform.position.ToVector2())
                {
                    var transformPos = transform.position;
                    transformPos.x = pos.x;
                    transformPos.y = pos.y;
                    transform.position = transformPos;
                }
            }
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
        }
    }
}