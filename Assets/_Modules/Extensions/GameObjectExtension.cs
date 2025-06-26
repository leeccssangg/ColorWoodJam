using UnityEngine;

public static class GameObjectExtension
{
    /// <summary>
    /// Check if gameobject has layer with name
    /// </summary>
    /// <param name="go"></param>
    /// <param name="layerName"></param>
    /// <returns></returns>
    public static bool CompareLayer(this GameObject go, string layerName)
    {
        return go.layer == LayerMask.NameToLayer(layerName);
    }

    /// <summary>
    /// Set layer for gameobject and its children
    /// </summary>
    /// <param name="go"></param>
    /// <param name="layer"></param>
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        foreach (Transform trans in go.transform.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layer;
        }
    }

    /// <summary>
    /// Set layer for gameobject, can include parent and children by layer name
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="layerName"></param>
    /// <param name="includeParent"></param>
    /// <param name="includeChildren"></param>
    public static void SetLayerRecursively(this GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        SetLayerRecursively(go, layer);
    }

    /// <summary>
    /// Using for shrinking effect (dead enemy, destroyed building,...)
    /// </summary>
    /// <param name="go"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="duration"></param>
    /// <param name="space"></param>
    /// <param name="onStart"></param>
    /// <param name="onComplete"></param>
    // public static void TranslateToGround(this GameObject go, Vector3 direction, float distance, float duration, Space space, Callback onStart = null
    //     , Callback onComplete = null)
    // {
    //     if (space == Space.World)
    //         LeanTween.move(go, go.transform.position + direction * distance, duration).setOnStart(() =>
    //         {
    //             if (onStart != null)
    //             {
    //                 onStart.Invoke();
    //             }
    //         }).setOnComplete(() =>
    //             {
    //                 if (onComplete != null)
    //                 {
    //                     onComplete.Invoke();
    //                 }
    //             }
    //         );
    //     else
    //         LeanTween.move(go, go.transform.position + direction * distance, duration).setOnStart(onStart.Invoke).setOnComplete(onComplete.Invoke);
    // }

    /// <summary>
    /// Raycasts the collider2D target.
    /// </summary>
    /// <returns>The collider2D target.</returns>
    /// <param name="go">Go.</param>
    /// <param name="direction">Direction.</param>
    /// <param name="layerName">Layer name.</param>
    /// <param name="maxDistance">Max distance.</param>
    public static GameObject RaycastCollider2DTarget(this GameObject go, Vector3 direction, string layerName,
        float maxDistance = float.PositiveInfinity)
    {
        var ray = new Ray(go.transform.position, direction);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, maxDistance, LayerMask.GetMask(layerName));

        if (hit2D.collider != null)
            return hit2D.collider.gameObject;

        return null;
    }
}