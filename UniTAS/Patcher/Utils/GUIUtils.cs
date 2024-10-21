using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class GUIUtils
{
    public static readonly GUILayoutOption[] EmptyOptions = [];

    public static Rect WindowRect(int width, int height)
    {
        return new(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height);
    }

    private static readonly GUIStyle LabelWithFontSize = new();
    private static readonly GUIContent LabelContent = new();

    private const int TextShadowOffset = 1;

    private static readonly (int, int)[] TextShadowOffsets =
    [
        new(TextShadowOffset, TextShadowOffset), new(-TextShadowOffset * 2, 0),
        new(0, -TextShadowOffset * 2), new(TextShadowOffset * 2, 0)
    ];

    /// <summary>
    /// Creates a shadowed text
    /// </summary>
    /// <param name="text">The text to render</param>
    /// <param name="fontSize">Font size</param>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <returns>Size of the text</returns>
    public static Vector2 ShadowedText(string text, int fontSize, int x, int y)
    {
        LabelWithFontSize.fontSize = fontSize;
        LabelWithFontSize.normal.textColor = Color.black;
        LabelContent.text = text;

        var size = LabelWithFontSize.CalcSize(LabelContent);

        var labelRect = new Rect(x, y, size.x, size.y);
        var labelRectOriginal = new Rect(labelRect);

        foreach (var (shadowOffsetX, shadowOffsetY) in TextShadowOffsets)
        {
            labelRect.x += shadowOffsetX;
            labelRect.y += shadowOffsetY;
            GUI.Label(labelRect, text, LabelWithFontSize);
        }

        LabelWithFontSize.normal.textColor = Color.white;
        GUI.Label(labelRectOriginal, text, LabelWithFontSize);

        return size;
    }
}