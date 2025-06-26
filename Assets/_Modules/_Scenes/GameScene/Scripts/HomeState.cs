using HomeViews;
using Mimi.Prototypes;

namespace GameScenes
{
    public class HomeState : BaseSceneState
    {
        protected override void OnInitialized()
        {
        }

        public override void Enter()
        {
            Presenter.GetViewPresenter<HomePresenter>().Show();
            PlayEnterTransition();
        }

        public override void Exit()
        {
            Presenter.GetViewPresenter<HomePresenter>().Hide();
        }
    }
}