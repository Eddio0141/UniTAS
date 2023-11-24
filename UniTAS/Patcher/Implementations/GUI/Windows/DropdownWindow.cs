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
        new(draggable: false, resizable: false, showTitle: false, closeButton: false, forceOnTop: true,
            forceFocus: true,
            layoutOptions: new[] { GUILayout.ExpandHeight(true), GUILayout.Height(10) }))

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

        var buttonHover = new Texture2D(1, 1);
        buttonHover.SetPixel(1, 1, new(0f, 0.5f, 1.0f));
        buttonHover.Apply();
        var buttonBg = new Texture2D(1, 1);
        buttonBg.SetPixel(1, 1, GUIUtils.StandardBgColour);
        buttonBg.Apply();

        _buttonStyle = new()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = new() { background = buttonBg, textColor = Color.white },
            hover = new() { background = buttonHover, textColor = Color.white },
            active = new() { background = buttonHover, textColor = Color.white },
            focused = new() { background = buttonHover, textColor = Color.white },
            fixedHeight = 25
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

    private readonly GUIStyle _buttonStyle;

    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        foreach (var entry in _entries)
        {
            var name = entry.Title;
            var action = entry.EntryFunction;
            if (!GUILayout.Button(name, _buttonStyle, GUIUtils.EmptyOptions)) continue;

            action();
            Close();
        }

        GUILayout.EndVertical();
    }
}