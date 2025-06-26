using System;
using FrogunnerGames;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WinViews
{
    public class WinView : BaseView
    {
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI coinText;

        public event Action OnClickNext;

        public override void Initialize()
        {
            base.Initialize();
            this.nextButton.onClick.AddListener(() => OnClickNext?.Invoke());
        }

        public void SetCoin(int coin)
        {
            this.coinText.text = StringNumber.IntToText(coin);
        }
    }
}