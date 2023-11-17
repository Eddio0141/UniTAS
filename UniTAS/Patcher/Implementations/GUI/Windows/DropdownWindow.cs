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
                windowBgColours[i] = GUIUtils.HoverColour;
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

    private bool _pendingClose;

    protected override void OnGUI()
    {
        // check window close by clicking elsewhere
        if (Event.current.type == EventType.Repaint)
        {
            var mousePos = _patchReverseInvoker.Invoke(() => UnityInput.Current.mousePosition);
            var screenHeight = _patchReverseInvoker.Invoke(() => Screen.height);
            var leftClick = _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButtonDown(0));
            var mousePosVector2 = new Vector2(mousePos.x, screenHeight - mousePos.y);

            if (WindowRect.Contains(mousePosVector2))
            {
                StaticLogger.Log.LogDebug($"{WindowRect}, {mousePosVector2}");
            }

            if (!WindowRect.Contains(mousePosVector2) && leftClick)
            {
                _pendingClose = true;
            }
        }

        if (_pendingClose && Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
        {
            Close();
            Event.current.Use();
        }

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