using System;
using System.Collections.Generic;
using System.Linq;
using Ability;
using DG.Tweening;
using Grids;
using Mimi.Prototypes.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public enum BlockType
{
    None,
    Square_1x1,
    Line_1x4,
    Line_1x2,
    Line_1x3,
    L_2x2,
    L_2x3,
}

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class Block : MonoBehaviour
{
    private static readonly int ColorShaderProperty = Shader.PropertyToID("_block_color");
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Outline outline;
    [SerializeField] private Transform checkCellTransform;
    [SerializeField] private GameObject[] inputGos;
    [SerializeField] private Transform[] checkGateTransforms;
    [SerializeField] private GameObject[] colliderBlocks;
    [SerializeField] private GameObject visualBlock;
    public Rigidbody Rigidbody => this.rb;
    public GameObject VisualBlock => this.visualBlock;

    [SerializeField, OnValueChanged("OnColorChanged")]
    private ColorId colorId;

    [SerializeField] private List<BaseAbility> abilities;

    private Transform trans;
    private Rigidbody rb;
    private MaterialPropertyBlock props;
    private bool isGetedInput;
    //private bool isFreed;

    public Vector3 Position => this.rb.position;
    public MeshRenderer MeshRenderer => this.meshRenderer;
    public event Action<Block> OnMoveCompleted;
    public event Action<Block> OnFree;
    public IReadOnlyList<BaseAbility> Abilities => this.abilities;
    private readonly HashSet<int> collidingBlockIds = new HashSet<int>(5);
    public bool IsCheckingGate;
    public Vector3 VisualPosition => this.VisualBlock.transform.position;

    private void Awake()
    {
        IgnoreCollisionBetweenOwnedColliders();
        SetInputStatus(false);
        this.trans = transform;
        this.rb = GetComponent<Rigidbody>();

        foreach (BaseAbility ability in abilities)
        {
            ability.Initialize(this);
        }
    }

    private void OnEnable()
    {
        foreach (BaseAbility ability in abilities)
        {
            ability.Begin();
            Debug.Log("BeginAbility: " + ability.GetType());
        }

        SetVisualBlock(this.visualBlock);
    }

    private void OnDisable()
    {
        foreach (BaseAbility ability in abilities)
        {
            ability.End();
        }
    }

    public void SetConstraint(RigidbodyConstraints constraints)
    {
        this.rb.constraints = constraints;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (this.collidingBlockIds.Contains(other.gameObject.GetInstanceID())) return;
        this.collidingBlockIds.Add(other.gameObject.GetInstanceID());
    }

    private void OnCollisionExit(Collision other)
    {
        if (!this.collidingBlockIds.Contains(other.gameObject.GetInstanceID())) return;
        this.collidingBlockIds.Remove(other.gameObject.GetInstanceID());
    }

    private void IgnoreCollisionBetweenOwnedColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length - 1; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[j]);
            }
        }
    }

    public void SetVelocity(Vector3 velocity)
    {
        this.rb.velocity = velocity;
    }

    public void MovePosition(Vector3 position)
    {
        this.rb.MovePosition(position);
    }

    public void AddForce(Vector3 force)
    {
        this.rb.AddForce(force, ForceMode.VelocityChange);
    }

    public void SetActiveCollision(bool active)
    {
        if (active)
        {
            this.rb.isKinematic = false;
            this.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            this.rb.isKinematic = true;
            this.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        this.rb.velocity = Vector3.zero;
    }

    public void SnapToGrid()
    {
        if (this.checkCellTransform == null)
        {
            Debug.LogError("tfCheckMove is null");
        }

        Ray ray = new Ray(this.checkCellTransform.position, Vector3.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, LayerMask.GetMask("Grid")))
        {
            var cell = hit.collider.GetComponentInParent<Cell>();
            Vector3 cellPosition = cell.WorldPosition;
            Vector3 offset = cellPosition - this.checkCellTransform.position;
            offset.z = 0f;
            this.trans.position += offset;
        }
    }

    public List<Cell> GetOccupiedCells()
    {
        List<Cell> cells = new List<Cell>(3);
        foreach (Transform checkTrans in this.checkGateTransforms)
        {
            Ray ray = new Ray(checkTrans.position, Vector3.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                var cell = hit.collider.GetComponentInParent<Cell>();
                if (cell == null) continue;
                cells.Add(cell);
            }
        }

        return cells;
    }

    public void Free(Vector3 destination)
    {
        // if (this.isFreed) return;
        // this.isFreed = true;
        Debug.Log("Free: " + this.name);
        destination.z = this.trans.position.z;
        Messenger.Broadcast(EventKey.BlockFree);
        GameObject currentVisualBlock = this.visualBlock;
        OnFree?.Invoke(this);
        if (this.IsContainLayer())
        {
            var vector3 = currentVisualBlock.transform.position;
            vector3.z = vector3.z - 0.5f;
            currentVisualBlock.transform.position = vector3;
        }

        // currentVisualBlock.transform.DOScale(Vector3.zero, 0.25f);
        currentVisualBlock.transform.DOMove(destination, 3f).SetSpeedBased().OnComplete(() =>
        {
            OnMoveCompleted?.Invoke(this);
            currentVisualBlock.gameObject.SetActive(false);
        });
    }

    public void FreeImmediately()
    {
        Messenger.Broadcast(EventKey.BlockFree);
        OnFree?.Invoke(this);
        OnMoveCompleted?.Invoke(this);
        this.gameObject.SetActive(false);
    }

    public void SetActiveInput(bool active)
    {
        foreach (GameObject inputGo in this.inputGos)
        {
            inputGo.SetActive(active);
        }
    }

    public void SetActiveCollider(bool active)
    {
        foreach (GameObject colliderBlock in this.colliderBlocks)
        {
            colliderBlock.SetActive(active);
        }
    }

    public void SetActiveOutline(bool active)
    {
        this.outline.enabled = active;
    }

    public void SetVisualBlock(GameObject visualBlock)
    {
        this.visualBlock = visualBlock;
        Messenger.Broadcast(EventKey.BlockChangeGraphic, this.visualBlock, this);
    }

    public bool CanMove(Vector3 dir, LayerMask layerMask, ColorId color)
    {
        if (color != this.colorId) return false;
        if (this.checkGateTransforms.Length == 0) return false;
        for (int i = 0; i < this.Abilities.Count; i++)
        {
            if (abilities[i] is IceLock iceLock)
            {
                if (iceLock.GetCurrentHp() > 0)
                {
                    return false;
                }
            }
        }

        List<RaycastHit> gateHits = new();
        for (int i = 0; i < this.checkGateTransforms.Length; i++)
        {
            Transform checkGate = this.checkGateTransforms[i];
            var ray = new Ray(checkGate.position, dir);
            Debug.DrawRay(checkGate.position, dir.normalized * 10f, Color.red, 1f);
            RaycastHit[] hits = Physics.RaycastAll(ray, 10f, layerMask);
            var validHits = Enumerable.ToArray(Enumerable.Where(hits,
                hit => !Enumerable.Contains(this.colliderBlocks, hit.collider.gameObject)));
            gateHits.AddRange(Enumerable.Where(validHits, hit => hit.collider.gameObject.CompareLayer(Layers.Gate)));
            var blockHits = Enumerable.ToArray(Enumerable.Where(validHits, hit =>
                (hit.collider.gameObject.CompareLayer(Layers.Block) ||
                 hit.collider.gameObject.CompareLayer(Layers.Wall))));
            if (blockHits.Length >= 1)
            {
                return false;
            }
        }

        for (int j = 0; j < gateHits.Count; j++)
        {
            if (gateHits[j].collider.gameObject.GetInstanceID() != gateHits[0].collider.gameObject.GetInstanceID())
            {
                return false;
            }

            if (gateHits[j].collider.gameObject.GetComponent<Gate>().colorId != this.colorId)
            {
                return false;
            }
        }

        return true;
    }

    public ColorId GetColorId() => this.colorId;

    public bool IsContainLayer()
    {
        foreach (BaseAbility ability in this.abilities)
        {
            if (ability is MultiLayer multiLayer && multiLayer.IsHaveLayers())
            {
                return true;
            }
        }

        return false;
    }
    public bool IsContainLayerWithColor(ColorId colorId)
    {
        if(!IsContainLayer()) return false;
        foreach (BaseAbility ability in this.abilities)
        {
            if (ability is MultiLayer multiLayer && multiLayer.IsContainColor(colorId))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsContainIceLock()
    {
        foreach (BaseAbility ability in this.abilities)
        {
            if (ability is IceLock iceLock && iceLock.GetCurrentHp() > 0)
            {
                return true;
            }
        }

        return false;
    }

    public MultiLayer GetMultiLayer()
    {
        foreach (BaseAbility ability in this.abilities)
        {
            if (ability is MultiLayer multiLayer)
            {
                return multiLayer;
            }
        }

        return null;
    }

    public bool HaveGotInput()
    {
        return this.isGetedInput;
    }

    public void SetInputStatus(bool isGetedInput)
    {
        this.isGetedInput = isGetedInput;
    }
    public void TriggerOnMoveCompleted()
    {
        OnMoveCompleted?.Invoke(this);
    }

    [Button]
    private void GetOutlineComponent()
    {
        this.outline = GetComponentInChildren<Outline>();
        this.outline.OutlineWidth = 6f;
        this.outline.OutlineColor = new Color(1f, 1f, 1f, 0.8f);
        this.outline.enabled = false;
    }

    public void SetColorId(ColorId colorId)
    {
        this.colorId = colorId;
        // ColorUtil.ApplyColorMaterial(this.meshRenderer, this.colorId);
    }

#if UNITY_EDITOR

    [Button]
    private void OnColorChanged()
    {
        ColorUtil.ApplyColorMaterial(this.meshRenderer, this.colorId);
    }

    [Button]
    private void GetAllAbilities()
    {
        this.abilities = GetComponents<BaseAbility>().ToList();
    }

#endif
}