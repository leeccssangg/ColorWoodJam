using System;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LoseViews
{
    public class LoseView : BaseView
    {
        [SerializeField] private Button homeButton;

        public event Action OnClickHome;

        public override void Initialize()
        {
            base.Initialize();
            this.homeButton.onClick.AddListener(() => OnClickHome?.Invoke());
        }
    }
}