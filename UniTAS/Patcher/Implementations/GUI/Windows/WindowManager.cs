using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
[ForceInstantiate]
public class WindowManager : Window, IWindowManager
{
    private readonly List<Window> _minimizedWindows = new();

    private int _prevMinimizedWindowsCount;

    private readonly IWindowFactory _windowFactory;

    public WindowManager(IUpdateEvents updateEvents, IPatchReverseInvoker reverseInvoker,
        IWindowFactory windowFactory) :
        base(new(updateEvents, null, reverseInvoker),
            new(showCloseButton: false, showMinimizeButton: false, defaultWindowRect: new(),
                windowName: "UniTAS",
                layoutOptions: new[]
                {
                    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)
                }))
    {
        _windowFactory = windowFactory;

        var separatorLine = new Texture2D(1, 1);
        separatorLine.SetPixel(0, 0, Color.gray);
        separatorLine.Apply();

        _separatorStyle = new()
        {
            margin = new(0, 0, 5, 5), fixedHeight = 3, normal = { background = separatorLine }
        };

        Show();
    }

    public void Minimize(Window window)
    {
        _minimizedWindows.Add(window);
    }

    private readonly GUILayoutOption[] _expandWidthOptions = { GUILayout.ExpandWidth(true) };

    private readonly GUIStyle _separatorStyle;

    protected override void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            if (_prevMinimizedWindowsCount != _minimizedWindows.Count)
            {
                WindowRect.height = 0;
                WindowRect.width = 0;
            }

            _prevMinimizedWindowsCount = _minimizedWindows.Count;
        }

        if (GUILayout.Button("Search", _expandWidthOptions))
        {
            _windowFactory.Create<WindowSearch>().Show();
        }

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        if (GUILayout.Button("Settings", GUIUtils.EmptyOptions))
        {
        }

        if (GUILayout.Button("Hotkeys", GUIUtils.EmptyOptions))
        {
        }

        GUILayout.EndHorizontal();

        if (_minimizedWindows.Count <= 0) return;

        // separator line
        GUILayout.Box(GUIContent.none, _separatorStyle, _expandWidthOptions);

        for (var i = 0; i < _minimizedWindows.Count; i++)
        {
            var window = _minimizedWindows[i];
            if (GUILayout.Button(window.WindowName, GUIUtils.EmptyOptions))
            {
                window.Show();
                _minimizedWindows.RemoveAt(i);
                i--;
            }
        }
    }
}