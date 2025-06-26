using Economy.Resources;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace Boosters
{
    public class BoosterPresenter : BaseViewPresenter
    {
        private readonly ResourceCollection resourceCollection;
        private readonly DialogManager dialogManager;
        private readonly RuntimeState runtimeState;
        private readonly ISaveManager saveManager;
        private BoosterView boosterView;

        public BoosterPresenter(BaseScenePresenter scenePresenter, Transform transform,
            ResourceCollection resourceCollection, DialogManager dialogManager, RuntimeState runtimeState,
            ISaveManager saveManager) : base(
            scenePresenter,
            transform)
        {
            this.resourceCollection = resourceCollection;
            this.dialogManager = dialogManager;
            this.runtimeState = runtimeState;
            this.saveManager = saveManager;
        }

        protected override void AddViews()
        {
            this.boosterView = AddView<BoosterView>();
            this.boosterView.TimerBoosterButton.OnClickBooster += TimerFreezeHandler;
            this.boosterView.HammerBoosterButton.OnClickBooster += HammerHandler;
            this.boosterView.VacuumBoosterButton.OnClickBooster += VacuumHandler;
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();
            Messenger.AddListener(EventKey.ReplayLevel, ReplayLevelHandler);
            UpdateBoosterAmount();
            this.resourceCollection.ResourceChanged += ResourceChangedHandler;
            IntroduceNewBoosters();
        }

        protected override void OnHide()
        {
            base.OnHide();
            Messenger.RemoveListener(EventKey.ReplayLevel, ReplayLevelHandler);
            this.resourceCollection.ResourceChanged -= ResourceChangedHandler;
            CancelBoosters();
        }

        private void IntroduceNewBoosters()
        {
            ShowNewBoosterDialog(this.boosterView.TimerBoosterButton);
            ShowNewBoosterDialog(this.boosterView.HammerBoosterButton);
            // ShowNewBoosterDialog(this.boosterView.VacuumBoosterButton, 30);
        }

        private void ShowNewBoosterDialog(BoosterButton boosterButton)
        {
            bool unlockBooster =
                this.runtimeState.LevelTop.Value >= boosterButton.BoosterData.UnlockLevelIndex;
            bool shouldIntroBooster =
                unlockBooster &&
                !this.runtimeState.ShowNewBoosterIds.Contains(
                    boosterButton.BoosterData.ResourceId);
            
            boosterButton.SetLock(!unlockBooster);
            if (shouldIntroBooster)
            {
                this.runtimeState.ShowNewBoosterIds.Add(boosterButton.BoosterData.ResourceId);
                if (this.dialogManager.TryShowModalDialogDelayShow(DialogId.UnlockBooster,
                        out NewBoosterDialog newBoosterDialog))
                {
                    newBoosterDialog.Init(boosterButton.BoosterData);
                    newBoosterDialog.Show();
                }

                this.saveManager.Save();
            }
        }

        private void ReplayLevelHandler()
        {
            CancelBoosters();
        }

        private void CancelBoosters()
        {
            this.boosterView.TimerBoosterButton.Booster.Cancel();
            this.boosterView.HammerBoosterButton.Booster.Cancel();
            // this.boosterView.VacuumBoosterButton.Booster.Cancel();
        }

        private void UpdateBoosterAmount()
        {
            this.boosterView.TimerBoosterButton.SetAmount(
                Mathf.CeilToInt(this.resourceCollection.GetAmount(ResourceId.FreezeTimer)));
            this.boosterView.HammerBoosterButton.SetAmount(
                Mathf.CeilToInt(this.resourceCollection.GetAmount(ResourceId.Hammer)));
            this.boosterView.VacuumBoosterButton.SetAmount(
                Mathf.CeilToInt(this.resourceCollection.GetAmount(ResourceId.Vacuum)));
        }

        private void ResourceChangedHandler(object sender, ResourceChangedEventArgs e)
        {
            UpdateBoosterAmount();
        }

        private void VacuumHandler()
        {
            UseBooster(this.boosterView.VacuumBoosterButton);
        }

        private void HammerHandler()
        {
            UseBooster(this.boosterView.HammerBoosterButton);
        }

        private void TimerFreezeHandler()
        {
            UseBooster(this.boosterView.TimerBoosterButton);
        }

        private void UseBooster(BoosterButton boosterButton)
        {
            if (boosterButton.Booster.IsBusy) return;
            string resourceId = boosterButton.BoosterData.ResourceId;
            if (this.resourceCollection.HasEnough(resourceId, 1))
            {
                this.resourceCollection.Sink(resourceId, 1,
                    ResourceAddress.New(resourceId, "gameplay"));
                boosterButton.Booster.Use();
            }
            else
            {
                if (this.dialogManager.TryShowModalDialogDelayShow(DialogId.BuyBooster,
                        out BuyBoosterDialog buyBoosterDialog))
                {
                    this.boosterView.Timer.PauseTimer();
                    buyBoosterDialog.Init(this.dialogManager, this.resourceCollection, boosterButton.BoosterData);
                    buyBoosterDialog.Show();
                    buyBoosterDialog.OnHiding += dialog => this.boosterView.Timer.ResumeTimer();
                }
            }
        }
    }
}