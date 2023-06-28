using BepInEx;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Interfaces.GUI;

/// <summary>
/// Base class for all windows.
/// Uses automatic layout.
/// </summary>
public abstract class Window
{
    protected Rect WindowRect;
    private int _windowId;
    private readonly UnityEngine.GUI.WindowFunction _windowUpdate;
    private bool _dragging;
    private Vector2 _dragOffset;
    private bool _showWindow;

    private readonly WindowConfig _config;

    private const int CLOSE_BUTTON_SIZE = 20;

    private readonly IUpdateEvents _updateEvents;
    private readonly IWindowManager _windowManager;

    public string WindowName => _config.WindowName;

    protected Window(WindowDependencies windowDependencies, WindowConfig config)
    {
        _updateEvents = windowDependencies.UpdateEvents;
        _windowManager = windowDependencies.WindowManager;
        _config = config ?? new();
        _windowUpdate = WindowUpdate;
        Init();
    }

    private void Init()
    {
        WindowRect = _config.DefaultWindowRect;
        _windowId = GetHashCode();
    }

    public void Show()
    {
        if (_showWindow) return;
        _updateEvents.OnGUIEventUnconditional += OnGUIUnconditional;
        _showWindow = true;
    }

    private void Close()
    {
        _updateEvents.OnGUIEventUnconditional -= OnGUIUnconditional;
        _showWindow = false;
    }

    private void Minimize()
    {
        _updateEvents.OnGUIEventUnconditional -= OnGUIUnconditional;
        _windowManager?.Minimize(this);
        _showWindow = false;
    }

    private void OnGUIUnconditional()
    {
        WindowRect = GUILayout.Window(_windowId, WindowRect, _windowUpdate, _config.WindowName,
            _config.LayoutOptions);
    }

    private void WindowUpdate(int id)
    {
        HandleDrag();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // close button
        if (_config.ShowCloseButton &&
            UnityEngine.GUI.Button(new(WindowRect.width - CLOSE_BUTTON_SIZE, 0f, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE),
                "x"))
        {
            Close();
        }

        if (_config.ShowMinimizeButton &&
            UnityEngine.GUI.Button(
                new(WindowRect.width - CLOSE_BUTTON_SIZE * 2, 0f, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE), "_"))
        {
            Minimize();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        OnGUI();

        CheckDrag();
    }

    private void HandleDrag()
    {
        if (!_dragging) return;

        // stupid hack to make window dragging smooth
        if (Event.current.type == EventType.Repaint)
        {
            var mousePos = (Vector2)UnityInput.Current.mousePosition;
            WindowRect.x = mousePos.x - _dragOffset.x;
            WindowRect.y = Screen.height - mousePos.y - _dragOffset.y;

            // just in case
            if (!UnityInput.Current.GetMouseButton(0))
            {
                _dragging = false;
            }

            ClampWindow();

            return;
        }

        if (Event.current.type != EventType.layout)
        {
            Event.current.Use();
        }
    }

    private void ClampWindow()
    {
        if (WindowRect.x < 0)
        {
            WindowRect.x = 0;
        }

        if (WindowRect.y < 0)
        {
            WindowRect.y = 0;
        }

        if (WindowRect.xMax > Screen.width)
        {
            WindowRect.x = Screen.width - WindowRect.width;
        }

        if (WindowRect.yMax > Screen.height)
        {
            WindowRect.y = Screen.height - WindowRect.height;
        }
    }

    private void CheckDrag()
    {
        if (_dragging || Event.current.type != EventType.MouseDown) return;

        var mousePos = (Vector2)UnityInput.Current.mousePosition;
        mousePos.y = Screen.height - mousePos.y;

        // are we dragging now?
        if (WindowRect.Contains(mousePos))
        {
            _dragging = true;
            _dragOffset = mousePos - new Vector2(WindowRect.x, WindowRect.y);
        }
    }

    protected abstract void OnGUI();
}