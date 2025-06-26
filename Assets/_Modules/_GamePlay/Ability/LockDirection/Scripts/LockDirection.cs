using Mimi.Prototypes.Events;
using UnityEngine;

namespace Ability
{
    public class LockDirection : BaseAbility
    {
        [SerializeField] private bool lockX;
        [SerializeField] private bool lockY;
        [SerializeField] private GameObject graphicArrow;

        private Block block;
        private RigidbodyConstraints startingConstraints;

        public override void Initialize(Block block)
        {
            this.block = block;
        }

        public override void Begin()
        {
            startingConstraints = block.Rigidbody.constraints;

            RigidbodyConstraints newConstraints = startingConstraints;

            if (lockX)
            {
                newConstraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX;
            }

            if (lockY)
            {
                newConstraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
            }

            block.SetConstraint(newConstraints);
            Messenger.AddListener<GameObject, Block>(EventKey.BlockChangeGraphic, BlockChangeGraphicHandler);
        }

        public override void End()
        {
            block.SetConstraint(startingConstraints);
            Messenger.RemoveListener<GameObject, Block>(EventKey.BlockChangeGraphic, BlockChangeGraphicHandler);
        }
        public void BlockChangeGraphicHandler(GameObject graphic, Block block)
        {
            if(block != this.block) return;
            graphicArrow.transform.SetParent(graphic.transform);
        }
    }
}