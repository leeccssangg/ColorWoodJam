using System;
using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Boosters
{
    public class NewBoosterDialog : SimpleAnimModalDialog
    {
        [SerializeField] private TMP_Text boosterNameText;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button closeButton;

        private event Action OnHide;
        private BoosterData booster;

        private void Awake()
        {
            if (this.closeButton != null) this.closeButton.onClick.AddListener(Hide);
        }

        public override void Hide()
        {
            base.Hide();
            OnHide?.Invoke();
        }

        public override void Show()
        {
            base.Show();
            this.descText.text = this.booster.Description;
            this.boosterNameText.text = this.booster.BoosterName;
            this.iconImage.sprite = this.booster.Icon;
        }

        public void AddOnHideAction(Action action)
        {
            OnHide += action;
        }

        public void Init(BoosterData boosterData)
        {
            this.booster = boosterData;
        }

        protected override void OnHideComplete()
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }
}