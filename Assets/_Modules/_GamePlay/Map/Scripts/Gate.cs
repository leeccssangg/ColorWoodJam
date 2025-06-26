using System.Collections.Generic;
using System.Linq;
using Grids;
using MEC;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.ServiceLocators;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

[SelectionBase]
public class Gate : MonoBehaviour
{
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private LayerMask cellLayer;
    [SerializeField] private LayerMask gateFxLayer;

    [SerializeField, OnValueChanged("OnColorChanged")]
    public ColorId colorId;

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MMScaleShaker scaleShaker;
    [SerializeField] private ParticleSystem[] grindingFxs;
    [SerializeField, SoundKey] private string grindingSound;

    private MaterialPropertyBlock props;
    private static readonly int BaseColorShaderProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int BlockColorShaderProperty = Shader.PropertyToID("_block_color");
    private Transform trans;
    private readonly Dictionary<int, CoroutineHandle> coroutineHandlesDic = new Dictionary<int, CoroutineHandle>();

    private void Awake()
    {
        this.trans = transform;
        SyncFxColorWithBlock();
    }

    private void OnDestroy()
    {
        CleanUpGateCheckCoroutines();
    }

    private void CleanUpGateCheckCoroutines()
    {
        if (this.coroutineHandlesDic.Count <= 0) return;
        foreach (CoroutineHandle coroutineHandle in this.coroutineHandlesDic.Values)
        {
            if (coroutineHandle.IsValid)
            {
                Timing.KillCoroutines(coroutineHandle);
            }
        }

        this.coroutineHandlesDic.Clear();
    }

    public void SyncFxColorWithBlock()
    {
        Material colorMaterial = this.meshRenderer.sharedMaterials.FirstOrDefault(x => !x.name.Contains("arrow"));
        this.props = new MaterialPropertyBlock();
        Color color = colorMaterial.GetColor(BlockColorShaderProperty);
        this.props.SetColor(BaseColorShaderProperty, color);

        foreach (ParticleSystem particle in this.grindingFxs)
        {
            var particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
            particleRenderer.SetPropertyBlock(this.props);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareLayer(Layers.Block)) return;
        var tetrisBlock = other.GetComponentInParent<Block>();
        if (tetrisBlock.GetColorId() != this.colorId) return;
        if (!tetrisBlock.HaveGotInput()) return;
        if (!this.coroutineHandlesDic.ContainsKey(tetrisBlock.GetInstanceID()))
        {
            CoroutineHandle coroutineHandle = Timing.RunCoroutine(_CheckDisposableBlock(tetrisBlock));
            this.coroutineHandlesDic.Add(tetrisBlock.GetInstanceID(), coroutineHandle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareLayer(Layers.Block)) return;
        var tetrisBlock = other.GetComponentInParent<Block>();
        if (tetrisBlock.GetColorId() != this.colorId) return;
        if (!tetrisBlock.HaveGotInput()) return;
        if (this.coroutineHandlesDic.ContainsKey(tetrisBlock.GetInstanceID()))
        {
            CoroutineHandle handle = this.coroutineHandlesDic[tetrisBlock.GetInstanceID()];
            this.coroutineHandlesDic.Remove(tetrisBlock.GetInstanceID());
            if (handle.IsValid)
            {
                Timing.KillCoroutines(handle);
            }
        }
    }

    private IEnumerator<float> _CheckDisposableBlock(Block block)
    {
        if (block.GetColorId() != this.colorId) yield break;
        Vector3 directionOut = -transform.right;
        bool isBlockCanMove = false;
        
        while (!isBlockCanMove)
        {
            isBlockCanMove = block.CanMove(directionOut, this.blockLayer, this.colorId);

            if (isBlockCanMove)
            {
                Debug.Log("Block is free");
                PlayGrindingFx(block);
                block.SetInputStatus(false);
                block.SetActiveCollider(false);
                block.SetActiveInput(false);
                block.SetActiveOutline(false);
                block.SetActiveCollision(false);
                block.SetVelocity(Vector3.zero);
                ServiceLocator.Global.Get<IAudioService>().PlaySound(this.grindingSound);
                block.SnapToGrid();
                Vector3 destination = block.VisualPosition + directionOut * 5f;
                block.Free(destination);
                this.coroutineHandlesDic.Remove(block.GetInstanceID());
            }
            yield return 0f;
        }
    }

    private void PlayGrindingFx(Block block)
    {
        List<Cell> blockCells = block.GetOccupiedCells();

        foreach (Cell cell in blockCells)
        {
            Vector3 raycastOrigin = cell.transform.position;
            Vector3 raycastDirection = -this.trans.right;
            Debug.DrawRay(raycastOrigin, raycastDirection, Color.red, 10f);

            if (Physics.Raycast(raycastOrigin, raycastDirection, out RaycastHit hit, 10f, this.gateFxLayer))
            {
                var particleSystem = hit.collider.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play(true);
                }
            }
        }
    }

#if UNITY_EDITOR
    [Button]
    private void OnColorChanged()
    {
        Material arrowMaterial = this.meshRenderer.sharedMaterials.FirstOrDefault(x => x.name.Contains("arrow"));
        int arrowMaterialIndex = GetMaterialIndex("arrow");
        int colorMaterialIndex = GetMaterialIndexExclude("arrow");
        ColorUtil.ApplyColorMaterial(this.meshRenderer, this.colorId);
        Material newColorMaterial = this.meshRenderer.sharedMaterials[0];
        var newMaterials = new Material[2];
        newMaterials[arrowMaterialIndex] = arrowMaterial;
        newMaterials[colorMaterialIndex] = newColorMaterial;
        this.meshRenderer.sharedMaterials = newMaterials;
    }

    private int GetMaterialIndex(string subName)
    {
        for (int i = 0; i < this.meshRenderer.sharedMaterials.Length; i++)
        {
            if (this.meshRenderer.sharedMaterials[i].name.Contains(subName))
            {
                return i;
            }
        }

        return -1;
    }

    private int GetMaterialIndexExclude(string excludeName)
    {
        for (int i = 0; i < this.meshRenderer.sharedMaterials.Length; i++)
        {
            if (!this.meshRenderer.sharedMaterials[i].name.Contains(excludeName))
            {
                return i;
            }
        }

        return -1;
    }
#endif
}