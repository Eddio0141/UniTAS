using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig(
    bool autoScale = false,
    GUILayoutOption[] layoutOptions = null,
    UnityEngine.Rect? defaultWindowRect = null,
    string windowName = null,
    bool showByDefault = false)
{
    public UnityEngine.Rect DefaultWindowRect { get; } = defaultWindowRect ?? GUIUtils.WindowRect(100, 100);

    public bool AutoScale { get; } = autoScale;

    public string WindowName { get; set; } = windowName ?? string.Empty;
    public GUILayoutOption[] LayoutOptions { get; } = layoutOptions ?? [];
    public bool ShowByDefault { get; } = showByDefault;
}