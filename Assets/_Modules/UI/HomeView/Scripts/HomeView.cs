using System;
using CoinViews;
using HeartBars;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HomeViews
{
    public class HomeView : BaseView
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private HeartBar heartBar;
        [SerializeField] private CoinBar coinBar;

        public event Action OnClickPlay;
        public event Action OnClickSetting;

        public HeartBar HeartBar => this.heartBar;
        public CoinBar CoinBar => this.coinBar;

        public override void Initialize()
        {
            base.Initialize();
            this.playButton.onClick.AddListener(() => OnClickPlay?.Invoke());
            this.settingButton.onClick.AddListener(() => OnClickSetting?.Invoke());
        }
    }
}