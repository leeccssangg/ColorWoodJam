using EnhancedUI.EnhancedScroller;
using FrogunnerGames;
using TMPro;
using UnityEngine;

namespace LevelScrollViews
{
    public class LevelCellView : EnhancedScrollerCellView
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject lockIconGo;

        public void SetData(int order, LevelCellData cellData)
        {
            this.levelText.text = StringNumber.IntToText(order);
            this.lockIconGo.SetActive(cellData.IsLocked);
        }
    }
}