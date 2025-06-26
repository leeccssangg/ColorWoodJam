using System;
using FrogunnerGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoinViews
{
    public class CoinBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private Button addButton;

        public event Action OnClickAdd;

        private void Awake()
        {
            this.addButton.onClick.AddListener(() => OnClickAdd?.Invoke());
        }

        public void SetCoinText(int coin)
        {
            this.coinText.text = StringNumber.IntToText(coin);
        }
    }
}