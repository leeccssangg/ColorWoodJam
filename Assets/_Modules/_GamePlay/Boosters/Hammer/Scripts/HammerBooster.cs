using Ability;
using DG.Tweening;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Boosters
{
    public class HammerBooster : BaseBooster
    {
        [SerializeField] private GameObject hammerGO;
        [SerializeField] private Camera castCamera;
        [SerializeField] private LayerMask blockInputLayerMask;
        [SerializeField] private bool isActive = false;
        [SerializeField] private PlayerGameplayInput playerGameplayInput;
        [SerializeField] private GameObject smashFxPrefab;
        [SerializeField] private MMCameraShaker cameraShaker;
        [ShowInInspector, ReadOnly] private Block selectedBlock;
        [SerializeField, SoundKey] private string grindingSound;
        [SerializeField] private GameObject instructionGo;

        private Vector3 offsetGOHammer = new Vector3(0, -1.25f, -2f);

        private void Awake()
        {
            hammerGO.SetActive(false);
            isActive = false;
        }

        [Button]
        protected override void OnUse()
        {
            //this.animator.Play(this.smashAnimationClip.name);
            //Smash(testBlock);
            instructionGo.SetActive(true);
            playerGameplayInput.SetActive(false);
            isActive = true;
        }

        protected override void OnCancel()
        {
            OnUseComplete();
        }

        private void Smash(Block block)
        {
            //PlayAnimHammer(block.transform.position);
            block.SetActiveOutline(true);
            if (TryProcessIceLock(block)) return;
            if (TryProcessMultiLayerBlock(block)) return;
            ProcessNormalBlock(block);
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

        private bool TryProcessIceLock(Block block)
        {
            foreach (var ability in block.Abilities)
            {
                if (ability is IceLock iceLock && iceLock.GetCurrentHp() > 0)
                {
                    ProcessIceLock(iceLock);
                    return true;
                }
            }

            return false;
        }

        private bool TryProcessMultiLayerBlock(Block block)
        {
            foreach (var ability in block.Abilities)
            {
                if (ability is MultiLayer multiLayerBlock && multiLayerBlock.IsHaveLayers())
                {
                    ProcessMultiLayerBlock(multiLayerBlock);
                    return true;
                }
            }

            return false;
        }

        private void ProcessIceLock(IceLock iceLock)
        {
            iceLock.DecreaseHp();
            OnUseComplete();
        }

        private void ProcessMultiLayerBlock(MultiLayer multiLayerBlock)
        {
            //multiLayerBlock.DecreaseHp();
            multiLayerBlock.BreakOutermostLayerImmediately();
            OnUseComplete();
        }

        private void ProcessNormalBlock(Block block)
        {
            block.FreeImmediately();
            OnUseComplete();
        }

        private void PlayAnimHammer(Block block)
        {
            var vector3 = block.Position + offsetGOHammer;
            hammerGO.transform.position = vector3;
            hammerGO.SetActive(true);
            Sequence hammerSequence = DOTween.Sequence();
            hammerSequence.Append(hammerGO.transform.DOLocalRotateQuaternion(
                    Quaternion.Euler(-110f, 0f, 0f), 0.25f)
                .SetEase(Ease.OutSine));
            hammerSequence.Append(hammerGO.transform.DOLocalRotateQuaternion(
                    Quaternion.Euler(0f, 0f, 0f), 0.15f)
                .SetEase(Ease.OutSine)
                .SetDelay(0.15f));
            hammerSequence.OnComplete(() =>
            {
                ServiceLocator.Global.Get<IAudioService>().PlaySound(this.grindingSound);
                ServiceLocator.Global.Get<IPoolService>()
                    .Spawn(this.smashFxPrefab, block.Position, Quaternion.identity);
                this.cameraShaker.ShakeCamera(0.1f, 0.2f, 0.01f, 0.1f, 0.1f, 0.1f, true);
                Smash(block);
            });
        }

        private void FingerDownHandler(LeanFinger finger)
        {
            if (!isActive) return;
            if (selectedBlock != null) return;
            Ray ray = this.castCamera.ScreenPointToRay(finger.ScreenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 50f, blockInputLayerMask))
            {
                var block = hit.collider.GetComponentInParent<Block>();
                selectedBlock = block;
                selectedBlock.SetActiveOutline(true);
                PlayAnimHammer(selectedBlock);
            }
        }

        private void FingerUpHandler(LeanFinger finger)
        {
            return;
        }

        private void FingerUpdateHandler(LeanFinger finger)
        {
            return;
        }

        private void OnUseComplete()
        {
            hammerGO.SetActive(false);
            isActive = false;
            selectedBlock.SetActiveOutline(false);
            selectedBlock = null;
            playerGameplayInput.SetActive(true);
            instructionGo.SetActive(false);
            IsBusy = false;
        }
    }
}