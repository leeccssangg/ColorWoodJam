using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grids
{
    [SelectionBase]
    public class Cell : MonoBehaviour
    {
        [SerializeField, InlineProperty] private CellIndex index;
        //[SerializeField] private SpriteRenderer spriteRenderer;

        public CellIndex Index => this.index;
        public Vector3 WorldPosition => trans.position;

        private Transform trans;

        private void Awake()
        {
            trans = transform;
        }
        
        public void SetIndex(int row, int column)
        {
            this.index.SetIndex(row, column);
        }

        public void SetColor(Color color)
        {
            //this.spriteRenderer.color = color;
        }
    }
}