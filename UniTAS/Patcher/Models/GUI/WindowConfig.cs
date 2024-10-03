using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig(
    GUILayoutOption[] layoutOptions = null,
    UnityEngine.Rect defaultWindowRect = default,
    string windowName = null)
{
    public UnityEngine.Rect DefaultWindowRect { get; } = defaultWindowRect == default ? new(0, 0, 100, 100) : defaultWindowRect;
    public string WindowName { get; } = windowName ?? string.Empty;
    public GUILayoutOption[] LayoutOptions { get; } = layoutOptions ?? [];
}
