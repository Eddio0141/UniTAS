using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public class WindowConfig(
    GUILayoutOption[] layoutOptions = null,
    UnityEngine.Rect? defaultWindowRect = null,
    string windowName = null,
    bool showByDefault = false,
    bool removeConfigOnClose = false)
{
    public UnityEngine.Rect DefaultWindowRect { get; } = defaultWindowRect ?? GUIUtils.WindowRect(100, 100);

    public string WindowName { get; set; } = windowName ?? string.Empty;
    public GUILayoutOption[] LayoutOptions { get; } = layoutOptions ?? [];
    public bool ShowByDefault { get; } = showByDefault;
    public bool RemoveConfigOnClose { get; } = removeConfigOnClose;
}