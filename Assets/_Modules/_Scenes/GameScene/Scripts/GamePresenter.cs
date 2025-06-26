using Boosters;
using GameplayViews;
using HomeViews;
using LoseViews;
using Mimi.Prototypes.UI;
using SettingViews;
using WinViews;

namespace Mimi.Prototypes
{
    public class GamePresenter : BaseScenePresenter<GameScene>
    {
        public override void Initialize(GameScene sceneController)
        {
            var context = sceneController.Context;
            base.Initialize(sceneController);
            var homePresenter = new HomePresenter(this, Transform, context.PlayerResources, context.EnergyPool,
                context.LevelOrder, context.RuntimeState);
            AddViewPresenter(homePresenter);

            var boosterPresenter = new BoosterPresenter(this, Transform, context.PlayerResources, context.DialogManager,
                context.RuntimeState, context.SaveManager);
            AddViewPresenter(boosterPresenter);

            var gameplayPresenter = new GameplayPresenter(this, Transform, boosterPresenter);
            AddViewPresenter(gameplayPresenter);

            var winPresenter = new WinViewPresenter(this, Transform);
            AddViewPresenter(winPresenter);

            var losePresenter = new LoseViewPresenter(this, Transform);
            AddViewPresenter(losePresenter);

            var settingPresenter = new SettingPresenter(this, Transform, context.AudioService, context.SaveManager,
                context.RuntimeState);
            AddViewPresenter(settingPresenter);
        }
    }
}