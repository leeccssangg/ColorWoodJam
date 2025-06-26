using System.Collections.Generic;
using MEC;
using Mimi;
using UnityEngine;
using UnityEngine.UI;

namespace Boosters
{
    public class TimerFreezeBooster : BaseBooster
    {
        [SerializeField] private PlayingState playingState;
        [SerializeField] private float secs = 10f;
        [SerializeField] private GameObject freezeFx;
        [SerializeField] private Image progressFillImage;
        // [SerializeField] private TextMeshProUGUI remainingSecsText;

        private CoroutineHandle freezeCoroutine;
        private float timer;

        protected override void OnUse()
        {
            if (this.freezeCoroutine.IsValid)
            {
                Timing.KillCoroutines(this.freezeCoroutine);
            }

            // this.remainingSecsText.text = StringNumber.IntToText(Mathf.CeilToInt(this.secs));
            this.progressFillImage.fillAmount = 1f;
            this.timer = this.secs;
            this.freezeFx.SetActive(true);
            this.playingState.Timer.PauseTimer();
            this.freezeCoroutine = Timing.RunCoroutine(_ProcessFreeze());
        }

        protected override void OnCancel()
        {
            if (this.freezeCoroutine.IsValid)
            {
                Timing.KillCoroutines(this.freezeCoroutine);
            }

            this.playingState.Timer.ResumeTimer();
            this.freezeFx.SetActive(false);
        }

        private IEnumerator<float> _ProcessFreeze()
        {
            while (true)
            {
                this.timer -= Time.deltaTime;
                this.timer = Mathf.Clamp(this.timer, 0f, this.secs);
                this.progressFillImage.fillAmount = this.timer / this.secs;
                // this.remainingSecsText.text = StringNumber.IntToText(Mathf.CeilToInt(this.timer));

                if (this.timer <= 0f)
                {
                    this.playingState.Timer.ResumeTimer();
                    this.freezeFx.SetActive(false);
                    IsBusy = false;
                    yield break;
                }

                yield return 0f;
            }
        }
    }
}