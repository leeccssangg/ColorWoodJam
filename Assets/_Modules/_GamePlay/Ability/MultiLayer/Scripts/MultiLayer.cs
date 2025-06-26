using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ability
{
    public class MultiLayer : BaseAbility
    {
        [SerializeField] private List<BlockLayer> layers;
        private Queue<BlockLayer> layerQueue;
        private Block block;


        public override void Initialize(Block block)
        {
            this.block = block;
            layerQueue = new Queue<BlockLayer>(layers);
            foreach (var layer in layers)
            {
                layer.Initialize();
            }

            if (layerQueue.Count > 0)
            {
                layerQueue.Peek().ActivateLayer();
            }
        }

        public override void Begin()
        {
            block.OnFree += BlockFreedHandler;
        }

        public override void End()
        {
            block.OnFree -= BlockFreedHandler;
        }

        public int GetLayerCount()
        {
            return layers.Count;
        }

        private void BlockFreedHandler(Block block)
        {
            if (!IsHaveLayers()) return;
            this.block.SetActiveInput(true);
            this.block.SetActiveCollider(true);
            BreakOutermostLayer();
        }

        private async void BreakOutermostLayer()
        {
            if (layerQueue.Count > 0)
            {
                // this.block.SnapToGrid();
                // this.block.SetActiveInput(false);
                // this.block.SetActiveCollider(false);
                var currentLayer = layerQueue.Dequeue();
                currentLayer.DeactivateLayer();
                if (layerQueue.Count > 0)
                {
                    layerQueue.Peek().ActivateLayer();
                }

                this.block.VisualBlock.transform.localScale = Vector3.one * 1.03f;
                this.block.SetVisualBlock(currentLayer.GraphicBlock);
                this.block.SetColorId(currentLayer.Color);
            }
        }

        public async void BreakOutermostLayerImmediately()
        {
            this.block.SetActiveCollider(false);
            this.block.SetActiveInput(false);
            this.block.VisualBlock.gameObject.SetActive(false);
            BreakOutermostLayer();
            this.block.TriggerOnMoveCompleted();
            await UniTask.WaitForSeconds(0.25f);
            this.block.SetActiveInput(true);
            this.block.SetActiveCollider(true);
        }
        public void BreakLayerWithColor(ColorId colorId)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].Color == colorId)
                {
                    layers[i].DeactiveLayerImmediately();
                    layers.RemoveAt(i);

                    // Rebuild the layerQueue after removing the layer
                    layerQueue = new Queue<BlockLayer>(layers);
                    this.block.TriggerOnMoveCompleted();
                    break;
                }
            }
        }

        public GameObject GetVisualBlockGO(ColorId colorId)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].Color == colorId)
                {
                    return layers[i].GetVisualBlock();
                }
            }

            return null;
        }
        public GameObject GetVisualLayerGO(ColorId colorId)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].Color == colorId)
                {
                    return layers[i].GetVisualLayer();
                }
            }

            return null;
        }

        public bool IsHaveLayers()
        {
            return layerQueue.Count > 0;
        }

        public bool IsContainColor(ColorId colorId)
        {
            foreach (var layer in layers)
            {
                if (layer.Color == colorId)
                {
                    return true;
                }
            }

            return false;
        }

        public List<ColorId> GetAllLayersColor()
        {
            List<ColorId> colors = new List<ColorId>();
            colors.Add(this.block.GetColorId());
            foreach (var layer in layers)
            {
                colors.Add(layer.Color);
            }

            return colors;
        }

        public void FreeLayerWithColor(ColorId colorId)
        {
            if (this.block.GetColorId() == colorId)
            {
                this.block.FreeImmediately();
            }
            else
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    if (layers[i].Color == colorId)
                    {
                        layers[i].DeactivateLayer();
                        var tmpList = layerQueue.ToList();
                        break;
                    }
                }
            }
            // foreach (var layer in layers)
            // {
            //     if (layer.Color == colorId)
            //     {
            //         layer.DeactivateLayer();
            //         layerQueue.Enqueue(layer);
            //         break;
            //     }
            // }
        }
    }

    [Serializable]
    public class BlockLayer
    {
        [SerializeField, OnValueChanged("OnColorChanged")]
        private ColorId colorId;

        [SerializeField] private GameObject visualLayer;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject visualBlock;
        [SerializeField] private MeshRenderer meshVisualBlock;
        [SerializeField] private GameObject graphicBlock;

        public GameObject GraphicBlock => graphicBlock;

        public ColorId Color => colorId;

        public void Initialize()
        {
            visualLayer.SetActive(false);
            visualBlock.SetActive(false);
            // SetColorMaterial();
        }

        public void ActivateLayer()
        {
            visualLayer.SetActive(true);
            visualBlock.SetActive(false);
        }

        public void DeactivateLayer()
        {
            visualLayer.SetActive(false);
            visualBlock.SetActive(true);
        }
        public void DeactiveLayerImmediately()
        {
            visualLayer.SetActive(false);
            visualBlock.SetActive(false);
        }
        public GameObject GetVisualBlock()
        {
            return visualBlock;
        }
        public GameObject GetVisualLayer()
        {
            return visualLayer;
        }


#if UNITY_EDITOR
        [Button]
        private void OnColorChanged()
        {
            ColorUtil.ApplyColorMaterial(this.meshRenderer, this.colorId);
            ColorUtil.ApplyColorMaterial(this.meshVisualBlock, this.colorId);
        }

        public void SetColorMaterial()
        {
            ColorUtil.ApplyColorMaterial(this.meshRenderer, this.colorId);
            ColorUtil.ApplyColorMaterial(this.meshVisualBlock, this.colorId);
        }
#endif
    }
}