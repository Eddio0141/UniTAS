using UnityEngine;

namespace UniTAS.Patcher.Extensions;

public static class ColorExtensions
{
    public static Color Flip(this Color color)
    {
        return new(1f - color.r, 1f - color.g, 1f - color.b, color.a);
    }
}