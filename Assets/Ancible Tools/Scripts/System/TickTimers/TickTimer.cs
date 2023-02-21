using System;
using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.TickTimers
{
    public delegate void TickUpdate(int ticks, int maxTicks);
    public delegate void TickFinished();
    public delegate void TimerDestroyed();

    [Serializable]
    public class TickTimer : IDisposable
    {
        public int TicksPerCycle;
        public int Loops;

        public TimerState State { get; private set; }
        public event TickUpdate OnTickUpdate = null;
        public event TickFinished OnTickFinished = null;
        public event TimerDestroyed OnDestroyed = null;
        public int TickCount => _tickCount;
        public int RemainingTicks => TicksPerCycle - _tickCount;

        private Action _applyAction = null;
        private Action _onFinish = null;
        private int _tickCount = 0;
        private int _loopCount = 0;

        public TickTimer(int ticksPerCycle, int loops, Action apply, Action onFinish, bool start = true, Action<int> onTickUpdate = null)
        {
            TicksPerCycle = ticksPerCycle;
            Loops = loops;
            _applyAction = apply;
            if (onFinish != null)
            {
                _onFinish = onFinish;
                OnTickFinished += Finish;
            }
            State = TimerState.Stopped;
            if (start)
            {
                Play();
            }
        }

        public TickTimer Play()
        {
            if (State != TimerState.Playing)
            {
                State = TimerState.Playing;
                this.Subscribe<UpdateTickMessage>(UpdateTick);
            }
            else
            {
                _tickCount = 0;
                _loopCount = 0;
            }

            return this;
        }

        public void SetCurrentTicks(int ticks)
        {
            _tickCount = ticks;
        }

        public void SetTicksPerCycle(int ticksPerCycle)
        {
            TicksPerCycle = ticksPerCycle;
            _tickCount = Mathf.Min(_tickCount, TicksPerCycle);
        }

        public TickTimer Stop(bool applyOnFinish = false)
        {
            if (State != TimerState.Stopped)
            {
                if (State == TimerState.Playing)
                {
                    this.Unsubscribe<UpdateTickMessage>();
                }
                State = TimerState.Stopped;
                _tickCount = 0;
                _loopCount = 0;
                if (applyOnFinish)
                {
                    OnTickFinished?.Invoke();
                }
            }

            return this;
        }

        public TickTimer Pause()
        {
            if (State == TimerState.Playing)
            {
                State = TimerState.Paused;
                this.Unsubscribe<UpdateTickMessage>();
            }

            return this;
        }

        public TickTimer Restart()
        {
            _tickCount = 0;
            _loopCount = 0;
            return this;
        }

        private void Tick()
        {
            _tickCount++;
            OnTickUpdate?.Invoke(_tickCount, TicksPerCycle);
            if (_tickCount >= TicksPerCycle)
            {
                _tickCount -= TicksPerCycle;
                _applyAction?.Invoke();
                if (Loops > -1)
                {
                    _loopCount++;
                    if (_loopCount > Loops)
                    {
                        State = TimerState.Stopped;
                        _tickCount = 0;
                        _loopCount = 0;
                        this.Unsubscribe<UpdateTickMessage>();
                        OnTickFinished?.Invoke();
                    }
                }
            }
        }

        private void Finish()
        {
            _onFinish?.Invoke();
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            Tick();
        }

        public void Destroy()
        {
            this.Unsubscribe<UpdateTickMessage>();
            _applyAction = null;
            _onFinish = null;
            OnDestroyed?.Invoke();
            Dispose();
        }

        public void Dispose()
        {
            _tickCount = 0;
            _loopCount = 0;
            TicksPerCycle = 0;
            Loops = 0;
            OnTickUpdate = null;
            OnTickFinished = null;
            OnDestroyed = null;
        }
    }
}