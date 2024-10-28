using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig(
    GUILayoutOption[] layoutOptions = null,
    UnityEngine.Rect defaultWindowRect = default,
    string windowName = null,
    bool showByDefault = false)
{
    public UnityEngine.Rect DefaultWindowRect { get; } =
        defaultWindowRect == default ? new(0, 0, 100, 100) : defaultWindowRect;

    public string WindowName { get; set; } = windowName ?? string.Empty;
    public GUILayoutOption[] LayoutOptions { get; } = layoutOptions ?? [];
    public bool ShowByDefault { get; } = showByDefault;
}