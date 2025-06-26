using GameScenes;
using Mimi;
using Mimi.Prototypes;
using Mimi.Prototypes.SaveLoad;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace SettingViews
{
    public class SettingPresenter : BaseViewPresenter
    {
        private readonly IAudioService audioService;
        private readonly ISaveManager saveManager;
        private readonly RuntimeState runtimeState;
        private SettingView settingView;

        public SettingPresenter(BaseScenePresenter scenePresenter, Transform transform, IAudioService audioService,
            ISaveManager saveManager, RuntimeState runtimeState) : base(scenePresenter,
            transform)
        {
            this.audioService = audioService;
            this.saveManager = saveManager;
            this.runtimeState = runtimeState;
        }

        protected override void AddViews()
        {
            this.settingView = AddView<SettingView>();
            this.settingView.OnClose += CloseHandler;
            this.settingView.OnClickHome += HomeClickedHandler;
            this.settingView.OnToggleMusic += OnToggleMusicHandler;
            this.settingView.OnToggleSound += OnToggleSoundHandler;
            this.settingView.OnToggleVibration += OnToggleVibrationHandler;

            this.settingView.SetMusicToggle(this.runtimeState.Music);
            this.settingView.SetSoundToggle(this.runtimeState.Sound);
            this.settingView.SetVibrationToggle(this.runtimeState.Vibration);
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();
            var gameScene = ScenePresenter.SceneController as GameScene;
            this.settingView.SetActiveHomeButton(gameScene.IsState<PlayingState>());
            this.settingView.SetMusicToggle(this.runtimeState.Music);
            this.settingView.SetSoundToggle(this.runtimeState.Sound);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        private void CloseHandler()
        {
            Hide();
        }

        private void OnToggleSoundHandler(bool value)
        {
            this.runtimeState.Sound = value;
            this.audioService.SetSoundVolPercentage(value ? 1f : 0f);
            this.saveManager.Save();
        }

        private void OnToggleMusicHandler(bool value)
        {
            this.runtimeState.Music = value;
            this.audioService.SetMusicVolPercentage(value ? 1f : 0f);
            this.saveManager.Save();
        }

        private void OnToggleVibrationHandler(bool value)
        {
            this.runtimeState.Vibration = value;
            this.saveManager.Save();
        }

        private void HomeClickedHandler()
        {
            var gameScene = ScenePresenter.SceneController as GameScene;
            gameScene.ChangeState<HomeState>();
            Hide();
        }
    }
}