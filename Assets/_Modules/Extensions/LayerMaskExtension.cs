using UnityEngine;

public static class LayerMaskExtension
{
    /// <summary>
    /// Remove a layer from layer mask
    /// </summary>
    /// <param name="layerMask"></param>
    /// <param name="layer"></param>
    /// <returns>Need to assign to original mask because LayerMask is a struct</returns>
    public static LayerMask RemoveLayer(this LayerMask layerMask, int layer)
    {
        return layerMask &= ~(1 << layer);
    }

    /// <summary>
    /// Add a layer to layer mask
    /// </summary>
    /// <param name="layerMask"></param>
    /// <param name="layer"></param>
    /// <returns>Need to assign to original mask because LayerMask is a struct</returns>
    public static LayerMask AddLayer(this LayerMask layerMask, int layer)
    {
        return layerMask |= (1 << layer);
    }

    /// <summary>
    /// Check if a gameobject had layer belongs to a layer mask
    /// </summary>
    /// <param name="go"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, GameObject go)
    {
        return mask == (mask | (1 << go.layer));
    }
}