using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class GUIUtils
{
    public static readonly GUILayoutOption[] EmptyOptions = [];

    public static Rect WindowRect(int width, int height)
    {
        return new(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height);
    }
}
