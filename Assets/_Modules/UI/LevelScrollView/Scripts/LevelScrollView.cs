using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace LevelScrollViews
{
    public class LevelScrollView : BaseView, IEnhancedScrollerDelegate
    {
        [SerializeField] private float cellSize;
        [SerializeField] private EnhancedScrollerCellView cellViewPrefab;
        [SerializeField] private EnhancedScroller levelScroller;

        private List<LevelCellData> cellDatas = new List<LevelCellData>();

        public int NumberOfVisibleCells => Mathf.Abs(this.levelScroller.StartDataIndex - this.levelScroller.EndDataIndex);

        public override void Initialize()
        {
            base.Initialize();
            this.levelScroller.Delegate = this;
        }

        public void LoadLevel(List<LevelCellData> cells)
        {
            this.cellDatas = cells;
            Refresh();
        }

        public void GoToLevel(int levelOrder, EnhancedScroller.CellViewPositionEnum position)
        {
            float scrollPos =
                this.levelScroller.GetScrollPositionForCellViewIndex(this.cellDatas.Count - 1 - levelOrder, position);
            this.levelScroller.SetScrollPositionImmediately(scrollPos);
        }
        
        public void GoToLevel(int levelOrder)
        {
            this.levelScroller.JumpToDataIndex(this.cellDatas.Count - 1 - levelOrder);
        }

        public void UpdateCellState(int topLevel)
        {
            for (int i = 0; i < this.cellDatas.Count; i++)
            {
                this.cellDatas[i].SetLocked(topLevel < i);
            }
        }

        public void Refresh()
        {
            this.levelScroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return this.cellDatas.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return this.cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(this.cellViewPrefab) as LevelCellView;
            int reverseOrder = this.cellDatas.Count - cellIndex - 1;
            LevelCellData cellData = this.cellDatas[reverseOrder];
            cellView.SetData(reverseOrder + 1, cellData);
            return cellView;
        }
    }
}