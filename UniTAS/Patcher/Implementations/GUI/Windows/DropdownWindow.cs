using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
public class DropdownWindow : Window
{
    private readonly DropdownEntry[] _entries;
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    public DropdownWindow(WindowDependencies windowDependencies, DropdownEntry[] entries, Vector2 position,
        IPatchReverseInvoker patchReverseInvoker) : base(
        windowDependencies,
        new(draggable: false, resizable: false, showTitle: false, closeButton: false))

    {
        windowDependencies.UpdateEvents.OnUpdateUnconditional += UpdateUnconditional;
        _entries = entries;
        _patchReverseInvoker = patchReverseInvoker;
        WindowRect = new(position.x, position.y, 200, 200);

        const int windowBgSize = 128;
        const int windowBgBorderSize = 1;
        var windowBg = new Texture2D(windowBgSize, windowBgSize);
        var windowBgColours = new Color[windowBgSize * windowBgSize];
        for (var i = 0; i < windowBgColours.Length; i++)
        {
            var x = i % windowBgSize;
            var y = i / windowBgSize;

            if (x < windowBgBorderSize || 0 < x - windowBgSize + 1 + windowBgBorderSize ||
                y < windowBgBorderSize || 0 < y - windowBgSize + 1 + windowBgBorderSize)
            {
                // its a border
                windowBgColours[i] = GUIUtils.HoldColour;
                continue;
            }

            windowBgColours[i] = GUIUtils.StandardBgColour;
        }

        windowBg.SetPixels(windowBgColours);
        windowBg.Apply();

        Style = new()
        {
            border = new(windowBgBorderSize, windowBgBorderSize, windowBgBorderSize, windowBgBorderSize),
            normal = new() { background = windowBg }
        };
    }

    private void UpdateUnconditional()
    {
        var mousePos = _patchReverseInvoker.Invoke(() => UnityInput.Current.mousePosition);
        var screenHeight = _patchReverseInvoker.Invoke(() => Screen.height);
        var leftClick = _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButtonDown(0));
        var mousePosVector2 = new Vector2(mousePos.x, screenHeight - mousePos.y);

        if (WindowRect.Contains(mousePosVector2) || !leftClick) return;

        Close();
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        foreach (var entry in _entries)
        {
            var name = entry.Title;
            var action = entry.EntryFunction;
            if (!GUILayout.Button(name, GUIUtils.EmptyOptions)) continue;

            action();
            Close();
        }

        GUILayout.EndVertical();
    }
}