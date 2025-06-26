using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Economy.Resources;
using EnergyTime;
using LevelScrollViews;
using Mimi;
using Mimi.Prototypes;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using SettingViews;
using UnityEngine;

namespace HomeViews
{
    public class HomePresenter : BaseViewPresenter
    {
        private readonly ResourceCollection resourceCollection;
        private readonly EnergyPool energyPool;
        private readonly ILevelOrder levelOrder;
        private readonly RuntimeState runtimeState;
        private HomeView homeView;
        private LevelScrollView levelScrollView;

        public HomePresenter(BaseScenePresenter scenePresenter, Transform transform,
            ResourceCollection resourceCollection, EnergyPool energyPool, ILevelOrder levelOrder,
            RuntimeState runtimeState) : base(scenePresenter,
            transform)
        {
            this.resourceCollection = resourceCollection;
            this.energyPool = energyPool;
            this.levelOrder = levelOrder;
            this.runtimeState = runtimeState;
        }

        protected override void AddViews()
        {
            this.homeView = AddView<HomeView>();
            this.levelScrollView = AddView<LevelScrollView>();
            this.homeView.OnClickPlay += ClickPlayHandler;
            this.homeView.OnClickSetting += ClickSettingHandler;
            this.energyPool.OnValueChanged += EnergyChangedHandler;
            this.resourceCollection.ResourceChanged += ResourceChangedHandler;
            ReloadLevelCells();
        }

        private void ClickSettingHandler()
        {
            ScenePresenter.GetViewPresenter<SettingPresenter>().Show();
        }

        private void ReloadLevelCells()
        {
            var cellDatas = new List<LevelCellData>();

            foreach (LevelInfo levelInfo in this.levelOrder.Levels)
            {
                var cellData = new LevelCellData(levelInfo.Difficulty, false);
                cellDatas.Add(cellData);
            }

            this.levelScrollView.LoadLevel(cellDatas);
        }

        private void ResourceChangedHandler(object sender, ResourceChangedEventArgs e)
        {
            if (e.Id == ResourceId.Coin)
            {
                this.homeView.CoinBar.SetCoinText(Mathf.CeilToInt(e.CurrentAmount));
            }
        }

        private void EnergyChangedHandler(int currentEnergy, int capacity)
        {
            this.homeView.HeartBar.SetCurrentEnergy(currentEnergy);

            if (this.energyPool.FullCapacity)
            {
                this.homeView.HeartBar.SetFullEnergyState();
            }
        }

        protected override void AddChildren()
        {
        }

        protected override async void OnShow()
        {
            base.OnShow();
            this.levelScrollView.UpdateCellState(this.runtimeState.LevelTop.Value);
            this.levelScrollView.Refresh();
            int currentLevel =
                Mathf.Clamp(
                    this.runtimeState.LevelTop.Value + this.levelScrollView.NumberOfVisibleCells,
                    0, this.levelOrder.LevelNumber - 1);

            int currentCoin = Mathf.CeilToInt(this.resourceCollection.GetAmount(ResourceId.Coin));
            this.homeView.CoinBar.SetCoinText(currentCoin);
            await UniTask.WaitForEndOfFrame(ScenePresenter);
            this.levelScrollView.GoToLevel(currentLevel);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        private void ClickPlayHandler()
        {
            // if (this.energyPool.HasEnough(1))
            // {
            var gameScene = ScenePresenter.SceneController as GameScene;
            gameScene.ChangeState<PlayingState>();
            // }
        }

        protected override void OnExecute()
        {
            base.OnExecute();
            if (!this.energyPool.FullCapacity)
            {
                this.homeView.HeartBar.SetEnergyTimer(this.energyPool.RemainingTimeToNextRecharge.Seconds);
            }
        }
    }
}