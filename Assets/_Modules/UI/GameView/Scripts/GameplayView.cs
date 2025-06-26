using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayViews
{
    public class GameplayView : BaseView
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private TextMeshProUGUI levelText;
        
        public event Action OnClickRetry;
        public event Action OnClickSetting;

        public override void Initialize()
        {
            base.Initialize();
            this.retryButton.onClick.AddListener(() => OnClickRetry?.Invoke());
            this.settingButton.onClick.AddListener(() => OnClickSetting?.Invoke());
        }

        public void SetLevelText(int level)
        {
            this.levelText.text = $"LEVEL {level + 1}";
        }
    }
}