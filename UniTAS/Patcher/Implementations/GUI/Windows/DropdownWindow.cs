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

    public DropdownWindow(WindowDependencies windowDependencies, DropdownEntry[] entries,
        IPatchReverseInvoker patchReverseInvoker) : base(windowDependencies,
        new(draggable: false, resizable: false, showTitle: false, closeButton: false))
    {
        _entries = entries;
        _patchReverseInvoker = patchReverseInvoker;
        WindowRect = WindowRectFromInfo();
    }

    private Rect WindowRectFromInfo()
    {
        var mousePos = _patchReverseInvoker.Invoke(() => UnityInput.Current.mousePosition);
        var screenHeight = _patchReverseInvoker.Invoke(() => Screen.height);
        return new(mousePos.x, screenHeight - mousePos.y, 200, 200);
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