using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public readonly struct WindowConfig
{
    public readonly Rect DefaultWindowRect = new(0, 0, 100, 100);
    public readonly string WindowName = null;
    public readonly GUILayoutOption[] LayoutOptions = null;
    public readonly GUIStyle Style = null;
    public readonly bool Draggable = true;
    public readonly bool Resizable = true;
    public readonly bool CloseButton = true;
    public readonly bool ForceOnTop = false;

    public WindowConfig(GUILayoutOption[] layoutOptions = null, GUIStyle style = null, Rect defaultWindowRect = default,
        string windowName = null, bool draggable = true, bool resizable = true, bool showTitle = true,
        bool closeButton = true, bool forceOnTop = false)
    {
        DefaultWindowRect = defaultWindowRect == default ? new(0, 0, 100, 100) : defaultWindowRect;
        layoutOptions ??= new GUILayoutOption[0];
        windowName ??= string.Empty;

        if (!showTitle)
        {
            windowName = string.Empty;

            // also force top border to be hidden
            style ??= new();
            style.border.top = 0;
        }

        WindowName = windowName;
        LayoutOptions = layoutOptions;
        Style = style;
        CloseButton = closeButton;
        Draggable = draggable;
        Resizable = resizable;
        ForceOnTop = forceOnTop;
    }
}