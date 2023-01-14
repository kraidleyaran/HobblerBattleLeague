using System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Timer
{
    public class UiTimerController : MonoBehaviour
    {
        [SerializeField] private Image _cooldownFillImage = null;

        private TimerType _timerType = TimerType.Timer;
        private TickTimer _timer = null;

        private Action _onFinish = null;

        public void Setup(TickTimer timer, TimerType type, Action onFinish)
        {
            _timerType = type;
            _timer = timer;
            _cooldownFillImage.fillClockwise = type == TimerType.Timer;
            _onFinish = onFinish;
            _timer.OnTickUpdate += OnTickUpdate;
            if (_onFinish != null)
            {
                _timer.OnTickFinished += OnTickFinish;
            }
        }

        private void OnTickUpdate(int currentTicks, int maxTicks)
        {
            
            switch (_timerType)
            {
                case TimerType.Timer:
                    _cooldownFillImage.fillAmount = (float)currentTicks / maxTicks; ;
                    break;
                case TimerType.Cooldown:
                    var remainingTicks = maxTicks - currentTicks;
                    _cooldownFillImage.fillAmount = (float)remainingTicks / maxTicks; ;
                    break;
            }

            _cooldownFillImage.raycastTarget = true;
        }

        private void OnTickFinish()
        {
            _cooldownFillImage.raycastTarget = false;
            _onFinish?.Invoke();
        }

        void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.OnTickUpdate -= OnTickUpdate;
                _timer.OnTickFinished -= OnTickFinish;
                _timer = null;
            }
            
        }
    }
}