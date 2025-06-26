using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public static class ColorExtension
{
    public static void SetAlpha(this Image image, float a)
    {
        Color tmp = image.color;
        tmp.a = a;
        image.color = tmp;
    }

    public static void SetAlpha(this SpriteRenderer sprite, float a)
    {
        Color tmp = sprite.color;
        tmp.a = a;
        sprite.color = tmp;
    }

    public static void SetAlpha(this Graphic graphic, float a)
    {
        Color tmp = graphic.color;
        tmp.a = a;
        graphic.color = tmp;
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.TrimStart('#');

        Color col = Color.black;

        if (hex.Length == 6)
        {
            col = new Color( // hardcoded opaque
                int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                255f);
        }
        else // assuming length of 8
        {
            col = new Color(
                int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber));
        }

        return col;
    }

    public static string GetHexRGB(this Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    public static string GetHexRGBA(this Color color)
    {
        return ColorUtility.ToHtmlStringRGBA(color);
    }
}