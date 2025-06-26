using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.SceneManagement;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using Mimi.StateMachine;
using UnityEngine;

namespace Mimi.Prototypes
{
    public abstract class BaseSceneState : State<MachineBehaviour>
    {
        [SerializeField] private GameObject enterTransitionPrefab;

        protected BaseScenePresenter Presenter { private set; get; }
        protected GameContext Context { private set; get; }

        private BaseSceneTransition sceneTransition;

        public void Initialize()
        {
            ServiceLocator.Global.Get<IPoolService>().Preload(this.enterTransitionPrefab, 1);
            OnInitialized();
        }

        protected abstract void OnInitialized();

        public override void Enter()
        {
            base.Enter();
            this.sceneTransition = ServiceLocator.Global.Get<IPoolService>()
                .Spawn<BaseSceneTransition>(this.enterTransitionPrefab);
        }

        protected void PlayEnterTransition()
        {
            if (this.sceneTransition != null)
            {
                this.sceneTransition.StartTransition();
            }
        }
    }
}