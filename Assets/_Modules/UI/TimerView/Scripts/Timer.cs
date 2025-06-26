using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Timers
{
    public class Timer
    {
        private CoroutineHandle coroutineHandle;
        private float timer;

        public event Action OnTimerEnd;

        public int RemainingSecs { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        public void StartTimer(float duration)
        {
            IsRunning = true;
            this.timer = duration;
            RemainingSecs = Mathf.CeilToInt(duration);
            this.coroutineHandle = Timing.RunCoroutine(_UpdateTimer());
        }

        public void StopTimer()
        {
            IsRunning = false;
            if (this.coroutineHandle.IsValid)
            {
                Timing.KillCoroutines(this.coroutineHandle);
            }
        }

        public void PauseTimer()
        {
            IsPaused = true;
        }

        public void ResumeTimer()
        {
            IsPaused = false;
        }

        private IEnumerator<float> _UpdateTimer()
        {
            while (true)
            {
                while (IsPaused)
                {
                    yield return 0f;
                }

                this.timer -= Time.deltaTime;
                RemainingSecs = Mathf.FloorToInt(this.timer);

                if (RemainingSecs <= 0)
                {
                    OnTimerEnd?.Invoke();
                    IsRunning = false;
                    yield break;
                }

                yield return 0f;
            }
        }
    }
}