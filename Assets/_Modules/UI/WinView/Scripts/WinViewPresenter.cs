using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace WinViews
{
    public class WinViewPresenter : BaseViewPresenter
    {
        private WinView winView;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform) : base(scenePresenter,
            transform)
        {
        }

        protected override void AddViews()
        {
            this.winView = AddView<WinView>();
            this.winView.OnClickNext += ClickWinHandler;
            Messenger.AddListener<LevelInfo>(EventKey.LevelWin, LevelWinHandler);
        }

        protected override void AddChildren()
        {
        }

        private void ClickWinHandler()
        {
            Messenger.Broadcast(EventKey.PlayNextLevel);
            Hide();
        }

        private void LevelWinHandler(LevelInfo levelInfo)
        {
            this.winView.SetCoin(levelInfo.Coin);
        }
    }
}