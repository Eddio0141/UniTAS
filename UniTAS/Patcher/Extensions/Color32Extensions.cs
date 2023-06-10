using UnityEngine;

namespace UniTAS.Patcher.Extensions;

public static class Color32Extensions
{
    public static bool EqualsColor(this Color32 color, Color32 other)
    {
        return color.r == other.r && color.g == other.g && color.b == other.b && color.a == other.a;
    }
}