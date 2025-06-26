using System.Collections.Generic;
using UnityEngine;
using System;
using Mimi.Prototypes.Events;
using TMPro;
using UnityEngine.Serialization;

namespace Ability
{
    public class IceLock : BaseAbility
    {
        [SerializeField] private GameObject iceLockGraphic;
        [SerializeField] private int hp;
        private Block block;
        private RigidbodyConstraints startingConstraints;
        private int currentHp;
        [SerializeField] private TextMeshProUGUI txtHp;
        
        public override void Initialize(Block block)
        {
            this.block = block;
            currentHp = hp;
            iceLockGraphic.SetActive(true);
            txtHp.SetText(currentHp.ToString());
            this.block.SetInputStatus(false);
        }

        public override void Begin()
        {
            Messenger.AddListener(EventKey.BlockFree, BlockFreedHandler);
            startingConstraints = block.Rigidbody.constraints;
            block.SetConstraint(RigidbodyConstraints.FreezeAll);
        }



        public override void End()
        {
            Messenger.RemoveListener(EventKey.BlockFree, BlockFreedHandler);
            block.SetConstraint(startingConstraints);
        }

        private void BlockFreedHandler()
        {
            DecreaseHp();
        }
        public void DecreaseHp()
        {
            if (currentHp > 0)
            {
                this.block.SetInputStatus(false);
                currentHp--;
                txtHp.SetText(currentHp.ToString());
                if (currentHp == 0)
                {
                    Break();
                }
            }
        }
        private void Break()
        {
            block.SetConstraint(startingConstraints);
            iceLockGraphic.SetActive(false);
        }
        public int GetCurrentHp()
        {
            return currentHp;
        }
    }
}