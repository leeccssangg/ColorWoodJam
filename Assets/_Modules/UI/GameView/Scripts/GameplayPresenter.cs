using Boosters;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using SettingViews;
using Timers;
using UnityEngine;

namespace GameplayViews
{
    public class GameplayPresenter : BaseViewPresenter
    {
        private readonly BoosterPresenter boosterPresenter;
        private GameplayView gameplayView;
        private TimerView timerView;
        private Timer currentTimer;

        public GameplayPresenter(BaseScenePresenter scenePresenter, Transform transform, BoosterPresenter boosterPresenter) : base(scenePresenter,
            transform)
        {
            this.boosterPresenter = boosterPresenter;
        }

        protected override void AddViews()
        {
            this.gameplayView = AddView<GameplayView>();
            this.timerView = AddView<TimerView>();
            this.gameplayView.OnClickRetry += RetryClickedHandler;
            this.gameplayView.OnClickSetting += SettingClickedHandler;
        }

        private void SettingClickedHandler()
        {
            ScenePresenter.GetViewPresenter<SettingPresenter>().Show();
        }

        private static void RetryClickedHandler()
        {
            Messenger.Broadcast(EventKey.ReplayLevel);
        }

        protected override void AddChildren()
        {
            AddChild(this.boosterPresenter);
        }

        public void SetLevelText(int level)
        {
            this.gameplayView.SetLevelText(level);
        }

        public void SetTimer(Timer timer, int totalSecs)
        {
            this.currentTimer = timer;
            this.timerView.SetTimer(totalSecs);
        }

        protected override void OnExecute()
        {
            base.OnExecute();
            if (this.currentTimer != null && this.currentTimer.IsRunning)
            {
                this.timerView.SetTimer(this.currentTimer.RemainingSecs);
            }
        }
    }
}