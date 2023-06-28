using System;
using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig
{
    public Rect DefaultWindowRect { get; }
    public string WindowName { get; }
    public bool ShowCloseButton { get; }
    public bool ShowMinimizeButton { get; }
    public GUILayoutOption[] LayoutOptions { get; }

    public WindowConfig(GUILayoutOption[] layoutOptions = null, Rect defaultWindowRect = default,
        string windowName = null, bool showCloseButton = true,
        bool showMinimizeButton = true)
    {
        DefaultWindowRect = new(Math.Max(defaultWindowRect.x, 0), Math.Max(defaultWindowRect.y, 0),
            Math.Max(defaultWindowRect.width, 100), Math.Max(defaultWindowRect.height, 100));
        WindowName = windowName ?? string.Empty;
        LayoutOptions = layoutOptions ?? new GUILayoutOption[0];
        ShowCloseButton = showCloseButton;
        ShowMinimizeButton = showMinimizeButton;
    }
}