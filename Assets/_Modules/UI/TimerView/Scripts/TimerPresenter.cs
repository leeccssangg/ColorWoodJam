using Mimi.Prototypes.UI;
using UnityEngine;

namespace Timers
{
    public class TimerPresenter : BaseViewPresenter
    {
        private TimerView timerView;
        private Timer currentTimer;

        public TimerPresenter(BaseScenePresenter scenePresenter, Transform transform) : base(scenePresenter, transform)
        {
        }

        protected override void AddViews()
        {
            this.timerView = AddView<TimerView>();
        }

        protected override void AddChildren()
        {
        }

        public void SetTimer(Timer timer)
        {
            this.currentTimer = timer;
        }

        protected override void OnExecute()
        {
            base.OnExecute();
            if (this.currentTimer != null)
            {
                this.timerView.SetTimer(this.currentTimer.RemainingSecs);
            }
        }
    }
}