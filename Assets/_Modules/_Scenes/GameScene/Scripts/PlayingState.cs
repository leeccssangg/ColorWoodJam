using Cysharp.Threading.Tasks;
using GameplayViews;
using LeveLoaders;
using Levels;
using LoseViews;
using Mimi.Audio;
using Mimi.Events;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.LevelManagement;
using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using Timers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WinViews;

namespace Mimi
{
    public class PlayingState : BaseSceneState
    {
        [SerializeField] private PlayerGameplayInput playerGameplayInput;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private ParticleSystem winParticle;
        [SerializeField, SoundKey] private string winSound;
        [SerializeField] private Volume volume;

        private LevelInfo currentLevelInfo;
        private LevelInfo nextLevelInfo;
        private GameObject levelRoot;
        private ILevel level;
        private ILevelLoader levelLoader;
        private Timer timer;
        private bool isWin;
        private bool isPlayerStartPlaying;
        private Vignette vignette;

        public Timer Timer => this.timer;

        [Button]
        private void ForceComplete()
        {
            LevelCompletedHandler();
        }

        [Button]
        private void ForcePlayLevel(int levelOrder)
        {
            PlayLevel(levelOrder);
        }

        protected override void OnInitialized()
        {
            this.timer = new Timer();
            this.timer.OnTimerEnd += TimerEndHandler;
            this.levelLoader = new AddressableLevelLoader(Context.LevelRepository);
            this.volume.profile.TryGet(out this.vignette);
        }

        private void TimerEndHandler()
        {
            if (this.isWin) return;
            playerGameplayInput.SetActive(false);
            Presenter.GetViewPresenter<LoseViewPresenter>().Show();
            Context.SaveManager.Save();
        }

        public override void Enter()
        {
            base.Enter();
            SetActiveVignette(true);
            this.playerGameplayInput.OnPlayerSelectBlock += PlayerSelectBlockHandler;
            Messenger.AddListener(EventKey.PlayNextLevel, PlayNextLevelHandler);
            Messenger.AddListener(EventKey.ReplayLevel, ReplayLevelHandler);
            Presenter.GetViewPresenter<GameplayPresenter>().Show();
            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
        }

        private void SetActiveVignette(bool active)
        {
            if (this.vignette != null)
            {
                this.vignette.active = active;
            }
        }

        private void ReplayLevelHandler()
        {
            this.isPlayerStartPlaying = false;
            this.timer.StopTimer();
            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
        }

        private void PlayerSelectBlockHandler()
        {
            if (this.isPlayerStartPlaying) return;
            this.isPlayerStartPlaying = true;
            this.timer.StartTimer(this.currentLevelInfo.Time);
        }

        public override void Exit()
        {
            base.Exit();
            SetActiveVignette(false);
            this.timer.StopTimer();
            this.playerGameplayInput.OnPlayerSelectBlock -= PlayerSelectBlockHandler;
            Messenger.RemoveListener(EventKey.PlayNextLevel, PlayNextLevelHandler);
            Messenger.RemoveListener(EventKey.ReplayLevel, ReplayLevelHandler);
            Presenter.GetViewPresenter<GameplayPresenter>().Hide();
        }

        private async void PlayLevel(int levelOrder)
        {
            this.isPlayerStartPlaying = false;
            this.isWin = false;
            CleanUpCurrentLevel();
            this.currentLevelInfo = Context.LevelOrder.GetByOrder(levelOrder);
            this.nextLevelInfo = Context.LevelOrder.GetNextLevel(levelOrder);
            this.levelLoader.Load(this.nextLevelInfo.Id);
            Presenter.GetViewPresenter<GameplayPresenter>().SetTimer(this.timer, this.currentLevelInfo.Time);
            Presenter.GetViewPresenter<GameplayPresenter>().SetLevelText(levelOrder);
            await PlayLevel(this.currentLevelInfo);
            ShowFeatureDialog();
        }

        private void ShowFeatureDialog()
        {
            bool hasTutShown = Context.RuntimeState.ShownTutIds.Contains(this.currentLevelInfo.NewFeature);
            if (hasTutShown) return;
            DialogId featureDialogId = DialogId.None;

            if (this.currentLevelInfo.NewFeature == 1)
            {
                featureDialogId = DialogId.Feature_ArrowBlock;
            }
            else if (this.currentLevelInfo.NewFeature == 2)
            {
                featureDialogId = DialogId.Feature_LayerBlock;
            }
            else if (this.currentLevelInfo.NewFeature == 3)
            {
                featureDialogId = DialogId.Feature_IceBlock;
            }

            if (featureDialogId != DialogId.None && Context.DialogManager.TryShowModalDialogOnce(featureDialogId,
                    out NotificationOkDialog featureDialog))
            {
                Context.RuntimeState.ShownTutIds.Add(this.currentLevelInfo.NewFeature);
                featureDialog.Show();
            }
        }

        private async UniTask PlayLevel(LevelInfo levelInfo)
        {
            GameObject levelPrefab = await this.levelLoader.Load(levelInfo.Id);
            this.levelRoot = ServiceLocator.Global.Get<IPoolService>().Spawn(levelPrefab);
            this.levelRoot.transform.SetPosition(0f, 0f, 0f);
            Context.EventPublisher.PublishAsync(new LevelStarted(levelInfo.Id));
            this.level = this.levelRoot.GetComponent<ILevel>();
            this.level.OnLevelCompleted += LevelCompletedHandler;
            this.gameCamera.fieldOfView = this.currentLevelInfo.CameraFov;
            this.playerGameplayInput.SetActive(true);
            await UniTask.WaitForEndOfFrame();
            PlayEnterTransition();
        }

        private async void LevelCompletedHandler()
        {
            this.timer.StopTimer();
            Messenger.Broadcast(EventKey.LevelWin, this.currentLevelInfo);
            Context.PlayerResources.Source(ResourceId.Coin, this.currentLevelInfo.Coin);
            this.isWin = true;
            this.playerGameplayInput.SetActive(false);
            this.level.OnLevelCompleted -= LevelCompletedHandler;
            Context.AudioService.PlaySound(this.winSound);
            this.winParticle.gameObject.SetActive(true);
            this.winParticle.Play(true);
            await UniTask.Delay(1500);
            Presenter.GetViewPresenter<GameplayPresenter>().Hide();
            Presenter.GetViewPresenter<WinViewPresenter>().Show();
            UpdateNextLevelIndex();
            Context.SaveManager.Save();
        }

        private void UpdateNextLevelIndex()
        {
            int nextLevelOrder = Mathf.Clamp(Context.RuntimeState.CurrentLevelOrder.Value + 1, 1,
                Context.LevelRepository.NumberOfLevels);
            Context.RuntimeState.CurrentLevelOrder.Value = nextLevelOrder;
            Context.RuntimeState.LevelTop.Value = nextLevelOrder;
        }

        private void PlayNextLevelHandler()
        {
            Presenter.GetViewPresenter<GameplayPresenter>().Show();
            Presenter.GetViewPresenter<WinViewPresenter>().Hide();
            PlayLevel(Context.RuntimeState.CurrentLevelOrder.Value);
        }

        private void CleanUpCurrentLevel()
        {
            if (this.levelRoot != null)
            {
                this.level.OnLevelCompleted -= LevelCompletedHandler;
                Destroy(this.levelRoot);
            }
        }
    }
}