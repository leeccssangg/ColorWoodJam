using GameScenes;
using Mimi.Prototypes;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace LoseViews
{
    public class LoseViewPresenter : BaseViewPresenter
    {
        private LoseView loseView;

        public LoseViewPresenter(BaseScenePresenter scenePresenter, Transform transform) : base(scenePresenter,
            transform)
        {
        }

        protected override void AddViews()
        {
            this.loseView = AddView<LoseView>();
            this.loseView.OnClickHome += HomeClickedHandler;
        }

        protected override void AddChildren()
        {
        }

        private void HomeClickedHandler()
        {
            var gameScene = ScenePresenter.SceneController as GameScene;
            gameScene.ChangeState<HomeState>();
            Hide();
        }
    }
}