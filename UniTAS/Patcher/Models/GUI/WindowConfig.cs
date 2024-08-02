using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig
{
    public UnityEngine.Rect DefaultWindowRect { get; }
    public string WindowName { get; }
    public GUILayoutOption[] LayoutOptions { get; }

    public WindowConfig(GUILayoutOption[] layoutOptions = null, UnityEngine.Rect defaultWindowRect = default,
        string windowName = null)
    {
        DefaultWindowRect = defaultWindowRect == default ? new(0, 0, 100, 100) : defaultWindowRect;
        WindowName = windowName ?? string.Empty;
        LayoutOptions = layoutOptions ?? new GUILayoutOption[0];
    }
}
