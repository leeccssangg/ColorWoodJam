using System;
using Coffee.UIEffects;
using FrogunnerGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Boosters
{
    public class BoosterButton : MonoBehaviour
    {
        [SerializeField] private BaseBooster booster;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI unlockLevelText;
        [SerializeField] private GameObject lockImageGo;
        [SerializeField] private GameObject plusSignGo;
        [SerializeField] private Image circleImage;
        [SerializeField] private Sprite greenCircleSprite;
        [SerializeField] private Sprite redCircleSprite;
        [SerializeField] private BoosterData boosterData;
        [SerializeField] private UIEffect[] grayscaleEffects;

        public event Action OnClickBooster;

        private bool isLocked;

        public BoosterData BoosterData => this.boosterData;
        public BaseBooster Booster => this.booster;

        private void Awake()
        {
            this.button.onClick.AddListener(() =>
            {
                if (!this.isLocked)
                {
                    OnClickBooster?.Invoke();
                }
            });
            this.unlockLevelText.text = "Lv." + this.boosterData.UnlockLevelIndex;
        }

        public void SetLock(bool locked)
        {
            this.isLocked = locked;
            this.lockImageGo.SetActive(locked);
            this.circleImage.gameObject.SetActive(!locked);
            this.unlockLevelText.gameObject.SetActive(locked);
        }

        public void SetAmount(int amount)
        {
            if (amount <= 0)
            {
                this.circleImage.sprite = this.redCircleSprite;
                this.amountText.text = string.Empty;
                this.plusSignGo.SetActive(true);
                SetGrayscale(true);
            }
            else
            {
                this.circleImage.sprite = this.greenCircleSprite;
                this.plusSignGo.SetActive(false);
                SetGrayscale(false);
                this.amountText.text = StringNumber.IntToText(amount);
            }
        }

        public void SetGrayscale(bool active)
        {
            foreach (UIEffect grayscaleEffect in this.grayscaleEffects)
            {
                grayscaleEffect.enabled = active;
            }
        }
    }
}