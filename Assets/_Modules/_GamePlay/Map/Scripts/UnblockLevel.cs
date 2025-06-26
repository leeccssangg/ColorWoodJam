using System;
using System.Collections.Generic;
using System.Linq;
using Ability;
using Levels;
using Mimi.Prototypes.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class UnblockLevel : MonoBehaviour, ILevel
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Gate[] gates;

    private int remainingBlocksCount;

    public event Action OnLevelCompleted;
    public event Action OnBlockFreedCompleted;

    private void Awake()
    {
        remainingBlocksCount = blocks.Length;
        for(int i = 0; i < blocks.Length; i++)
        {
            var multiLayer = blocks[i].GetMultiLayer();
            if(multiLayer != null)
            {
                remainingBlocksCount+= multiLayer.GetLayerCount();
            }
        }

        foreach (var block in blocks)
        {
            block.OnMoveCompleted += BlockMoveCompletedHandler;
        }
    }
    
    private void BlockMoveCompletedHandler(Block block)
    {
        remainingBlocksCount--;
        OnBlockFreedCompleted?.Invoke();
        bool hasWon = remainingBlocksCount == 0;
        Debug.Log(remainingBlocksCount);
        if (hasWon)
        {
            OnLevelCompleted?.Invoke();
        }
    }

    private void OnEnable()
    {
        Messenger.Broadcast(EventKey.UpdateLevel,this);
    }

    public List<Block> GetBlocks()
    {
        return blocks.ToList();
    }


#if UNITY_EDITOR
    [Button]
    public void GetAllMapEntities()
    {
        blocks = GetComponentsInChildren<Block>();
        gates = GetComponentsInChildren<Gate>();
    }
#endif
}