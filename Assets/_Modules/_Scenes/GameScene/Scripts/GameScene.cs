using Cysharp.Threading.Tasks;
using Mimi.Games.Ads;
using Mimi.Games.Plugins;
using UnityEngine;

namespace Mimi.Prototypes
{
    public class GameScene : BaseGameScene
    {
        public override void RequestAssets()
        {
        }

        protected override void AddLocalPlugins(CompositePlugin pluginInstaller)
        {
        }

        protected override async UniTask OnEnter()
        {
            await base.OnEnter();
            if (Context.SessionRecorder.IsFirstSession)
            {
                ChangeState<PlayingState>();
            }
        }
    }
}