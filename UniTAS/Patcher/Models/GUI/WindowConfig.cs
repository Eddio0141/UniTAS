using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig
{
    public Rect DefaultWindowRect { get; }
    public string WindowName { get; }
    public GUILayoutOption[] LayoutOptions { get; }

    public WindowConfig(GUILayoutOption[] layoutOptions = null, Rect defaultWindowRect = default,
        string windowName = null)
    {
        DefaultWindowRect = defaultWindowRect == default ? new(0, 0, 100, 100) : defaultWindowRect;
        WindowName = windowName ?? string.Empty;
        LayoutOptions = layoutOptions ?? new GUILayoutOption[0];
    }
}