using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.GUI;

/// <summary>
/// Base class for all windows.
/// Uses automatic layout.
/// </summary>
public abstract class Window : IOnGUI
{
    private Rect _windowRect;

    protected abstract Rect DefaultWindowRect { get; }
    protected abstract string WindowName { get; }

    protected Window()
    {
        Init();
    }

    private void Init()
    {
        _windowRect = DefaultWindowRect;
    }

    public void OnGUI()
    {
        _windowRect = GUILayout.Window(0, _windowRect, RenderWindow, WindowName);
    }

    private void RenderWindow(int id)
    {
        OnGUI(id);

        // make window draggable
        UnityEngine.GUI.DragWindow();
    }

    protected virtual void OnGUI(int id)
    {
    }
}