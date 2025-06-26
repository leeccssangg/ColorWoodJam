using System;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SettingViews
{
    public class SettingView : BaseView
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Button homeButton;

        public event Action OnClose;
        public event Action OnClickHome;
        public event Action<bool> OnToggleSound;
        public event Action<bool> OnToggleMusic;
        public event Action<bool> OnToggleVibration;

        public override void Initialize()
        {
            base.Initialize();
            this.homeButton.onClick.AddListener(() => OnClickHome?.Invoke());
            this.closeButton.onClick.AddListener(() => OnClose?.Invoke());
            this.soundToggle.onValueChanged.AddListener(value => OnToggleSound?.Invoke(value));
            this.musicToggle.onValueChanged.AddListener(value => OnToggleMusic?.Invoke(value));
        }

        public void SetMusicToggle(bool value)
        {
            this.musicToggle.isOn = value;
        }

        public void SetSoundToggle(bool value)
        {
            this.soundToggle.isOn = value;
        }

        public void SetVibrationToggle(bool value)
        {
            this.vibrationToggle.isOn = value;
        }

        public void SetActiveHomeButton(bool active)
        {
            this.homeButton.gameObject.SetActive(active);
        }
    }
}