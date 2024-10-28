using UniTAS.Patcher.Extensions;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class GUILayoutUtils
{
    private const int TextShadowOffset = 1;

    private static readonly (int, int)[] TextShadowOffsets =
    [
        new(TextShadowOffset, TextShadowOffset), new(-TextShadowOffset * 2, 0),
        new(0, -TextShadowOffset * 2), new(TextShadowOffset * 2, 0)
    ];

    private static readonly GUIContent LabelContent = new();

    /// <summary>
    /// Creates a shadowed label
    /// </summary>
    public static void ShadowedLabel(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        LabelContent.text = text;

        var labelRectOriginal = GUILayoutUtility.GetRect(LabelContent, style, options);
        var labelRect = new Rect(labelRectOriginal);

        var originalColor = style.normal.textColor;
        style.normal.textColor = style.normal.textColor.Flip();
        foreach (var (shadowOffsetX, shadowOffsetY) in TextShadowOffsets)
        {
            labelRect.x += shadowOffsetX;
            labelRect.y += shadowOffsetY;
            GUI.Label(labelRect, text, style);
        }

        style.normal.textColor = originalColor;
        GUI.Label(labelRectOriginal, text, style);
    }

    /// <summary>
    /// Creates a shadowed label
    /// </summary>
    public static void ShadowedLabel(string text)
    {
        ShadowedLabel(text, GUI.skin.label);
    }
}