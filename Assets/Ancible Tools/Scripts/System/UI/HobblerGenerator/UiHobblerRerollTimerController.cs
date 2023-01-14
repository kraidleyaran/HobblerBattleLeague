using System;
using Assets.Resources.Ancible_Tools.Scripts.System.TickTimers;
using MessageBusLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Resources.Ancible_Tools.Scripts.System.UI.HobblerGenerator
{
    public class UiHobblerRerollTimerController : MonoBehaviour
    {
        [SerializeField] private UiFillBarController _timerFillbar = null;
        [SerializeField] private Color _fillColor = Color.yellow;

        public TickTimer Timer { get; private set; }
        private bool _hovered = false;

        public void Setup(TickTimer timer)
        {
            Timer = timer;
            Timer.OnTickUpdate += UpdateTick;
            UpdateTick(Timer.TickCount, Timer.TicksPerCycle);
        }

        private void UpdateTick(int current, int max)
        {
            var remainingTicks = max - current;
            var remainingSeconds = 0;
            var percent = 0f;
            if (remainingTicks > 0)
            {
                remainingSeconds = (int)(TickController.WorldTickRate * remainingTicks);
                percent = (float)remainingTicks / max;
            }
            else
            {
                percent = 1f;
                remainingSeconds = (int) (TickController.WorldTickRate * max);
            }
            _timerFillbar.Setup(percent, $"{remainingSeconds}", _fillColor);
        }

        public void Destroy()
        {
            Timer.OnTickUpdate -= UpdateTick;
            Timer = null;
        }
    }
}