using System;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Mimi.Debugging.UnityGizmos;
#endif

public class PlayerGameplayInput : MonoBehaviour
{
    [SerializeField, SoundKey] private string pickupSound;
    [SerializeField, SoundKey] private string dropSound;
    [SerializeField] private Camera castCamera;
    [SerializeField] private LayerMask blockInputLayerMask;

    [ShowInInspector, ReadOnly] private Block selectedBlock;
    [ShowInInspector, ReadOnly] private bool IsDragging => selectedBlock != null;
    [ShowInInspector, ReadOnly] private bool isActive;

    private Plane movePlane;
    private Vector3 offset = Vector3.zero;

    public event Action OnPlayerSelectBlock;

    public void SetActive(bool active)
    { 
        if (this.isActive == !active)
        {
            this.isActive = active;
            ResetInput();
        }
    }

    private void ResetInput()
    {
        ResetSelectedBlock();
    }

    private void ResetSelectedBlock()
    {
        if (selectedBlock == null) return;
        selectedBlock.SnapToGrid();
        selectedBlock = null;
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
        LeanTouch.OnFingerUpdate += FingerUpdateHandler;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        if (!isActive || IsDragging) return;
        Ray ray = this.castCamera.ScreenPointToRay(finger.ScreenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, blockInputLayerMask))
        {
            var block = hit.collider.GetComponentInParent<Block>();
            selectedBlock = block;
            if (!selectedBlock.HaveGotInput())
            {
                selectedBlock.SetInputStatus(!selectedBlock.IsContainIceLock());
            }

            movePlane = new Plane(Vector3.forward, hit.point);
            offset = hit.point - selectedBlock.Position;
            selectedBlock.SetActiveCollision(true);
            selectedBlock.SetActiveOutline(true);
            ServiceLocator.Global.Get<IAudioService>().PlaySound(this.pickupSound);
            OnPlayerSelectBlock?.Invoke();
            Messenger.Broadcast(EventKey.PlayerSelectBlock, selectedBlock);
        }
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (!isActive || !IsDragging) return;
        ServiceLocator.Global.Get<IAudioService>().PlaySound(this.dropSound);
        selectedBlock.SetActiveCollision(false);
        selectedBlock.SnapToGrid();
        selectedBlock.SetActiveOutline(false);
        selectedBlock = null;
        offset = Vector3.zero;
    }

    [SerializeField] private float MoveThreshold = 0.02f;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float inputStepSpeed = 100f;

    private float fingerSpeedScale;

    private void FingerUpdateHandler(LeanFinger finger)
    {
        return;
        if (!isActive || !IsDragging) return;
        Ray ray = this.castCamera.ScreenPointToRay(finger.ScreenPosition);

        if (movePlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 targetPos = hitPoint - offset;
            Vector3 diff = targetPos - this.selectedBlock.Position;

#if UNITY_EDITOR
            VisualDebugger.DebugCircle(hitPoint, Vector3.forward, Color.red, 0.2f, 10f);
            VisualDebugger.DebugCircle(targetPos, Vector3.forward, Color.black, 0.2f, 10f);
#endif

            if (diff.magnitude >= MoveThreshold)
            {
                this.fingerSpeedScale = 20f + Mathf.Clamp01(finger.ScreenDelta.magnitude) * 5f;
                // Debug.Log(finger.ScaledDelta.magnitude + " " + finger.ScreenDelta.magnitude + " " + finger.LastSnapshotScaledDelta.magnitude + " " + finger.LastSnapshotScreenDelta.magnitude);
                // Vector3 stepPos = Vector3.MoveTowards(selectedBlock.Position, targetPos, Time.deltaTime * 50f * fingerSpeedScale);
                // Vector3 moveVector = stepPos - selectedBlock.Position;

                // this.selectedBlock.SetVelocity(newVelocity);
                Vector3 stepPos = Vector3.SmoothDamp(this.selectedBlock.Position, targetPos, ref this.velocity,
                    this.smoothTime);


                // if (this.selectedBlock.IsCollided)
                // {
                //     
                //     // Vector3 newVelocity = Vector3.ClampMagnitude(this.velocity, 20f);
                //     this.selectedBlock.SetVelocity(this.velocity);
                // }
                // else
                // {
                //     this.selectedBlock.MovePosition(stepPos);
                // }


                // Vector3 stepPos = Vector3.SmoothDamp(this.selectedBlock.Position, targetPos, ref this.velocity,
                //     this.smoothTime);

                // Vector3 stepPos = Vector3.MoveTowards(this.selectedBlock.Position, targetPos,
                //     Time.deltaTime * this.inputStepSpeed * this.fingerSpeedScale);
                this.velocity = stepPos - this.selectedBlock.Position;
                float currentSpeed = Mathf.Clamp(this.fingerSpeedScale * this.speed, this.speed, 2000f);
                this.selectedBlock.Rigidbody.velocity = this.velocity.normalized * (currentSpeed * Time.deltaTime);
            }
            else
            {
                this.selectedBlock.Rigidbody.velocity = Vector3.zero;
            }
        }
    }

    private Vector3 graphicVelocity;

    private void Update()
    {
        // if (this.selectedBlock != null)
        // {
        //     this.selectedBlock.transform.position = Vector3.SmoothDamp(this.selectedBlock.transform.position,
        //         this.selectedBlock.Position, ref this.graphicVelocity,
        //         this.smoothTime);
        // }
    }
}