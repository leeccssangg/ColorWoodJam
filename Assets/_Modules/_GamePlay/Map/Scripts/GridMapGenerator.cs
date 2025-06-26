using System.Reflection;
using Grids;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MapEditors
{
    [RequireComponent(typeof(GridMap))]
    public class GridMapGenerator : MonoBehaviour
    {
        [SerializeField] private Transform cellRoot;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField, MinValue(1)] private int row;
        [SerializeField, MinValue(1)] private int column;
        [SerializeField, MinValue(0.1f)] private float cellSize;
        
        public int Row => row;
        public int Column => column;

#if UNITY_EDITOR
        [Button]
        private void Generate()
        {
            var gridMap = GetComponent<GridMap>();
            this.cellRoot.DestroyChildren();
            SpawnGridGameObjects();
            Cell[] cells = this.cellRoot.GetComponentsInChildren<Cell>();
            SetCellsToMap(gridMap, cells);
        }
        
        private void SpawnGridGameObjects()
        {
            float colOffset = this.cellSize / 2f;
            float rowOffset = this.cellSize / 2f;
            int colorFillIndex = 0;
            
            for (int i = 0; i < this.row; i++)
            {
                colorFillIndex++;
                
                for (int k = 0; k < this.column; k++)
                {
                    colorFillIndex++;
                    Vector3 gridPos = Vector3.zero;
                    gridPos.x = (k - this.column / 2f) * this.cellSize + colOffset;
                    gridPos.y = (i - this.row / 2f) * this.cellSize + rowOffset;

                    var gridGo = PrefabUtility.InstantiatePrefab(this.cellPrefab) as GameObject;
                    gridGo.transform.parent = this.cellRoot;
                    gridGo.transform.localRotation = Quaternion.identity;
                    gridGo.transform.localPosition = gridPos;
                    gridGo.name = "Grid " + i + "-" + k;
                    var cell = gridGo.GetComponent<Cell>();
                    cell.SetIndex(i, k);
                    cell.SetColor(colorFillIndex % 2 == 0 ? Color.white : Color.gray);
                    cell.transform.localScale =  Vector3.one* this.cellSize;
                }
            }
        }
        
        private static void SetCellsToMap(GridMap gridMap, Cell[] cells)
        {
            FieldInfo cellsField = gridMap.GetType().GetField("cells", BindingFlags.Instance | BindingFlags.NonPublic);
            cellsField.SetValue(gridMap, cells);
        }
#endif
    }
}
