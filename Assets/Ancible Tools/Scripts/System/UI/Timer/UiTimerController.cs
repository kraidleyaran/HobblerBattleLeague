using System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.Timer
{
    public class UiTimerController : MonoBehaviour
    {
        public TickTimer Timer { get; private set; }

        [SerializeField] private Image _cooldownFillImage = null;

        private TimerType _timerType = TimerType.Timer;

        private Action _onFinish = null;

        public void Setup(TickTimer timer, TimerType type, Action onFinish)
        {
            _timerType = type;
            Timer = timer;
            _cooldownFillImage.fillClockwise = type == TimerType.Timer;
            _onFinish = onFinish;
            Timer.OnTickUpdate += OnTickUpdate;
            if (_onFinish != null)
            {
                Timer.OnTickFinished += OnTickFinish;
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
            if (Timer != null)
            {
                Timer.OnTickUpdate -= OnTickUpdate;
                Timer.OnTickFinished -= OnTickFinish;
                Timer = null;
            }
            
        }
    }
}