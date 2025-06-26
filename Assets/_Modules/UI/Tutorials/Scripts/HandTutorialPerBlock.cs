using DG.Tweening;
using Mimi.Prototypes.Events;
using UnityEngine;

namespace Tutorials
{
    public class HandTutorialPerBlock : MonoBehaviour
    {
        [SerializeField] private Block targetBlock;
        [SerializeField] private Transform handTrans;
        [SerializeField] private Transform targetTrans;
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private float delay = 0.3f;

        private void OnEnable()
        {
            Messenger.AddListener<Block>(EventKey.PlayerSelectBlock, PlayerSelectBlockHandler);
        }

        private void OnDisable()
        {
            Messenger.RemoveListener<Block>(EventKey.PlayerSelectBlock, PlayerSelectBlockHandler);
        }

        private void Awake()
        {
            this.handTrans.DOMove(this.targetTrans.position, this.moveDuration).SetDelay(this.delay)
                .SetLoops(-1, LoopType.Restart);
        }

        private void PlayerSelectBlockHandler(Block block)
        {
            if (block != this.targetBlock) return;
            gameObject.SetActive(false);
        }
    }
}