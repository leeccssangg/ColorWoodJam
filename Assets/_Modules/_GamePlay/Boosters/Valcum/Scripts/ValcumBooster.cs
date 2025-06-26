using System;
using System.Collections.Generic;
using System.Linq;
using Ability;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.Prototypes.Events;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Boosters
{
    public class ValcumBooster : BaseBooster
    {
        [SerializeField] private GameObject valcumGO;
        [SerializeField] private Camera castCamera;
        [SerializeField] private LayerMask blockInputLayerMask;
        [SerializeField] private Block selectedBlock;
        [SerializeField] private PlayerGameplayInput playerGameplayInput;
        [SerializeField] private Transform StartPos;
        [SerializeField] private Transform EndPos;
        [SerializeField] private Transform BlockMovePos;
        [SerializeField, SoundKey] private string grindingSound;
        [SerializeField] private List<Block> blocks;
        
        private ColorId colorId;
        private List<Block> listNormalBlocks;
        private List<Block> listMultiLayerBlocks;
        private Action<UnblockLevel> OnLevelChange;
        private bool isActive = false;

        private void Awake()
        {
            valcumGO.SetActive(false);
            isActive = false;
        }
        [Button]
        protected override void OnUse()
        {
            playerGameplayInput.SetActive(false);
            isActive = true;
        }

        protected override void OnCancel()
        {
            // Goi reset trang thai cua booster o day khi cancel giua chung. Vd dang dung booster thi thoat ra home
        }

        private void OnEnable()
        {
            Messenger.AddListener<UnblockLevel>(EventKey.UpdateLevel,SetUpBlocks);
            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
            LeanTouch.OnFingerUpdate += FingerUpdateHandler;
        }
        private void OnDisable()
        {
            Messenger.RemoveListener<UnblockLevel>(EventKey.UpdateLevel,SetUpBlocks);
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
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
                Valcum(selectedBlock.GetColorId());
                //PlayAnimHammer(selectedBlock);
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
            //valcumGO.SetActive(false);
            valcumGO.transform.DOMove(StartPos.position, 0.15f).OnComplete(()
                =>
            {
                valcumGO.gameObject.SetActive(false);
                isActive = false;
                selectedBlock.SetActiveOutline(false);
                selectedBlock = null;
                playerGameplayInput.SetActive(true);
                //instructionGo.SetActive(false);
                IsBusy = false;
            });
        }
        private void SetUpBlocks(UnblockLevel level)
        {
            List<Block> levelBlock = level.GetBlocks();
            this.blocks = new List<Block>(levelBlock);
        }
        private void Valcum(ColorId colorId)
        {
            this.colorId = colorId;
            valcumGO.gameObject.SetActive(true);
            valcumGO.transform.DOMove(EndPos.position, 0.5f).OnComplete(async () =>
            {
                ProcessNormalBlockWithColorId(this.colorId);
                ProcessMultiLayerBlockWithColorId(this.colorId);
                await UniTask.WaitForSeconds(0.35f);
                OnUseComplete();
            });
        }
        private async void ProcessNormalBlockWithColorId(ColorId colorId)
        {
            listNormalBlocks = blocks.Where(block => block.GetColorId() == colorId).ToList();
            for(int i = 0;i< listNormalBlocks.Count;i++)
            {
                var block = listNormalBlocks[i];
                GameObject visualBlock = block.VisualBlock.gameObject;
                visualBlock.transform.DOScale(0, 0.15f);
                visualBlock.transform.DOMove(BlockMovePos.position, 0.15f).OnComplete(
                    () =>
                    {
                        if (block.IsContainLayer())
                        {
                            block.GetMultiLayer().BreakOutermostLayerImmediately();
                        }
                        else
                        {
                            block.FreeImmediately();
                        }
                        ServiceLocator.Global.Get<IAudioService>().PlaySound(this.grindingSound);
                    });
                await UniTask.WaitForSeconds(0.1f);
            }
        }
        private async void ProcessMultiLayerBlockWithColorId(ColorId colorId)
        {
            listMultiLayerBlocks = blocks.Where(block => block.IsContainLayerWithColor(colorId)).ToList();
            for (int i = 0; i < listMultiLayerBlocks.Count; i++)
            {
                var block = listMultiLayerBlocks[i];
                GameObject visualBlock = block.GetMultiLayer().GetVisualBlockGO(colorId);
                block.GetMultiLayer().GetVisualLayerGO(colorId).gameObject.SetActive(false);
                visualBlock.gameObject.SetActive(true);
                visualBlock.transform.DOScale(0, 0.15f);
                visualBlock.transform.DOMove(BlockMovePos.position, 0.15f).OnComplete(
                    () =>
                    {
                        block.GetMultiLayer().BreakLayerWithColor(colorId);
                        ServiceLocator.Global.Get<IAudioService>().PlaySound(this.grindingSound);
                    });
                await UniTask.WaitForSeconds(0.1f);
            }
        }

        
    }
}