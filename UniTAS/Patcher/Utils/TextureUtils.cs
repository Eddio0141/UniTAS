using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class TextureUtils
{
    public static Texture2D MakeSolidColourTexture(int width, int height, Color colour)
    {
        var pix = new Color[width * height];
        for (var i = 0; i < pix.Length; i++)
            pix[i] = colour;
        var result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}