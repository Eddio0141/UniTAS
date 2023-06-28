using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI;

[Singleton]
[ForceInstantiate]
public class WindowManager : Window, IWindowManager
{
    private readonly List<Window> _minimizedWindows = new();

    private readonly Texture2D _separatorLine;

    public WindowManager(IUpdateEvents updateEvents) :
        base(updateEvents,
            new(showCloseButton: false, showMinimizeButton: false, defaultWindowRect: new(0, 0, 100, 100),
                windowName: "UniTAS",
                layoutOptions: new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) }), null)
    {
        _separatorLine = new(1, 1);
        _separatorLine.SetPixel(0, 0, Color.gray);
        _separatorLine.Apply();

        Show();
    }

    public void Minimize(Window window)
    {
        _minimizedWindows.Add(window);
    }

    protected override void OnGUI()
    {
        if (GUILayout.Button("Search", GUILayout.ExpandWidth(true)))
        {
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Settings"))
        {
        }

        if (GUILayout.Button("Hotkeys"))
        {
        }

        GUILayout.EndHorizontal();

        if (_minimizedWindows.Count > 0)
        {
            // separator line
            GUILayout.Box(GUIContent.none, new GUIStyle
            {
                margin = new(0, 0, 5, 5), fixedHeight = 3, normal = { background = _separatorLine }
            }, GUILayout.ExpandWidth(true));

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
}