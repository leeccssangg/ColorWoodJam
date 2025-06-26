using System;
using Economy.Resources;
using FrogunnerGames;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Boosters
{
    public class BuyBoosterDialog : SimpleAnimModalDialog
    {
        [SerializeField] private TMP_Text boosterNameText;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text costText;

        private event Action OnHide;
        private ResourceCollection resources;
        private BoosterData booster;
        private DialogManager dialogManager;

        private void Awake()
        {
            if (this.closeButton != null) this.closeButton.onClick.AddListener(Hide);
            this.buyButton.onClick.AddListener(ClickBuyHandler);
        }

        private void ClickBuyHandler()
        {
            bool enough = this.resources.HasEnough(ResourceId.Coin, this.booster.Cost);

            if (enough)
            {
                this.resources.Sink(this.booster.ResourceId, this.booster.Cost,
                    ResourceAddress.New(this.booster.ResourceId, "popup"));
                this.resources.Source(this.booster.ResourceId, this.booster.NumberPerPack);

                if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                        out AutoHideNotificationDialog notEnoughDialog))
                {
                    notEnoughDialog.SetText($"Receive {this.booster.BoosterName} x{this.booster.NumberPerPack}");
                }

                Hide();
            }
            else
            {
                if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                        out AutoHideNotificationDialog notEnoughDialog))
                {
                    notEnoughDialog.SetText("Not enough coin to buy this booster!");
                }
            }
        }

        public override void Hide()
        {
            base.Hide();
            OnHide?.Invoke();
        }

        public override void Show()
        {
            base.Show();
            this.costText.text = StringNumber.IntToText(this.booster.Cost);
            this.descText.text = this.booster.Description;
            this.boosterNameText.text = this.booster.BoosterName;
            this.iconImage.sprite = this.booster.Icon;
            this.amountText.text = "x" + this.booster.NumberPerPack;
            this.costText.color =
                this.resources.HasEnough(ResourceId.Coin, this.booster.Cost) ? Color.white : Color.red;
        }

        public void AddOnHideAction(Action action)
        {
            OnHide += action;
        }

        public void Init(DialogManager dialogManager, ResourceCollection resourceCollection, BoosterData boosterData)
        {
            this.dialogManager = dialogManager;
            this.resources = resourceCollection;
            this.booster = boosterData;
        }

        protected override void OnHideComplete()
        {
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }
}