using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CachedEveryThing 
{
    private static Dictionary<int, ICollider> CacheCollider = new();

    public static ICollider GetColliderUnit(this Collider collider)
    {
        int id = collider.gameObject.GetInstanceID();
        if (CacheCollider.TryGetValue(id, out ICollider dynamicUnit))
        {
            return dynamicUnit;
        }

        ICollider newUnit = collider.gameObject.GetComponent<ICollider>();
        CacheCollider.Add(id, newUnit);
        return newUnit;
    }

    private static Dictionary<int , Block> CacheTerisBlock = new();

    public static Block GetTerisBlockUnit(this Collider collider)
    {
        int id = collider.gameObject.GetInstanceID();
        if (CacheTerisBlock.TryGetValue(id, out Block dynamicUnit))
        {
            return dynamicUnit;
        }

        Block newUnit = collider.gameObject.GetComponent<Block>();
        CacheTerisBlock.Add(id, newUnit);
        return newUnit;
    }
}
