using System;
using UnityEngine;

namespace Grids
{
    [Serializable]
    public class CellIndex
    {
        [SerializeField] private int row;
        [SerializeField] private int column;

        public int Row => this.row;

        public int Column => this.column;

        public void SetIndex(int row, int col)
        {
            this.row = row;
            this.column = col;
        }

        public void SetIndex(CellIndex other)
        {
            this.row = other.row;
            this.column = other.column;
        }

        public bool CompareIndex(CellIndex other)
        {
            return (Row == other.Row && Column == other.Column) ? true : false;
        }

        public CellIndex()
        {
            this.row = 0;
            this.column = 0;
        }

        public CellIndex(int row, int col)
        {
            this.row = row;
            this.column = col;
        }

        public CellIndex(CellIndex cellIndex)
        {
            this.row = cellIndex.Row;
            this.column = cellIndex.Column;
        }
    }
}