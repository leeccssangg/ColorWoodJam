using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum ColorId
{
    None = -1,
    Color0,
    Color1,
    Color2,
    Color3,
    Color4,
    Color5,
    Color6,
    Color7,
    Color8,
    Color9,
    Color10,
}

public static class ColorUtil
{
    public static Color GetColorById(ColorId colorId)
    {
        switch (colorId)
        {
            case ColorId.Color0: return new Color(191f / 255f, 10f / 255f, 10f / 255f, 1f) * 0.5f;
            case ColorId.Color1: return new Color(5f / 255f, 8f / 255f, 191f / 255f, 1f) * 0.5167446f;
            case ColorId.Color2: return new Color(26f / 255f, 191f / 255f, 0f, 1f) * 0.5354605f;
            case ColorId.Color3: return new Color(191f / 255f, 38f / 255f, 3f / 255f, 1f) * 2.5f;
            case ColorId.Color4: return new Color(191f / 255f, 9f / 255f, 3f / 255f, 1f) * 2.9f;
            case ColorId.Color5: return new Color(9f / 255f, 51f / 255f, 5f / 255f, 1f);
            case ColorId.Color6: return new Color(0f, 255f / 255f, 177f / 255f, 1f);
            case ColorId.Color7: return new Color(42f / 255f, 8f / 255f, 191f / 255f, 1f) * 2.5f;
            case ColorId.Color8: return new Color(153f / 255f, 76f / 255f, 0f, 1f);
            case ColorId.Color9: return Color.gray * 1.2f;
            case ColorId.Color10: return Color.white * 2f;
            default: return Color.black;
        }
    }

#if UNITY_EDITOR
    public static void ApplyColorMaterial(MeshRenderer meshRenderer, ColorId colorId)
    {
        switch (colorId)
        {
            case ColorId.Color0:
                SetMaterialByName(meshRenderer, "block_blue");
                break;
            case ColorId.Color1:
                SetMaterialByName(meshRenderer, "block_cyan");
                break;
            case ColorId.Color2:
                SetMaterialByName(meshRenderer, "block_dark_green");
                break;
            case ColorId.Color3:
                SetMaterialByName(meshRenderer, "block_green");
                break;
            case ColorId.Color4:
                SetMaterialByName(meshRenderer, "block_orange");
                break;
            case ColorId.Color5:
                SetMaterialByName(meshRenderer, "block_purple");
                break;
            case ColorId.Color6:
                SetMaterialByName(meshRenderer, "block_red");
                break;
            case ColorId.Color7:
                SetMaterialByName(meshRenderer, "block_yellow");
                break;
            case ColorId.Color8:
                SetMaterialByName(meshRenderer, "block_lightblue");
                break;
            case ColorId.Color9:
                SetMaterialByName(meshRenderer, "block_pink");
                break;
            case ColorId.Color10:
            default:
                SetMaterialByName(meshRenderer, "block_yellow");
                break;
        }
    }
    
    private static void SetMaterialByName(MeshRenderer meshRenderer, string materialName)
    {
        string[] guids = AssetDatabase.FindAssets(materialName);
        var mats = new List<Material>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null) continue;
            mats.Add(material);
        }

        meshRenderer.materials = mats.ToArray();
    }
#endif
}