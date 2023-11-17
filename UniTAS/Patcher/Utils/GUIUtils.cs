using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class GUIUtils
{
    public static readonly GUILayoutOption[] EmptyOptions = new GUILayoutOption[0];

    public static readonly Color StandardBgColour = new(0.25f, 0.25f, 0.25f);
    public static readonly Color HoverColour = new(0.5f, 0.5f, 0.5f);

    public static Rect WindowRect(int width, int height)
    {
        return new(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height);
    }
}