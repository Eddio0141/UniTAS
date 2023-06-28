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
    private Rect _windowRect;
    private int _windowId;
    private readonly UnityEngine.GUI.WindowFunction _windowUpdate;
    private bool _dragging;
    private Vector2 _dragOffset;
    private bool _closed;

    private readonly WindowConfig _config;

    private const int CLOSE_BUTTON_SIZE = 20;

    private readonly IUpdateEvents _updateEvents;
    private readonly IWindowManager _windowManager;

    public string WindowName => _config.WindowName;

    protected Window(IUpdateEvents updateEvents, WindowConfig config, IWindowManager windowManager)
    {
        _updateEvents = updateEvents;
        _windowManager = windowManager;
        _config = config ?? new();
        _windowUpdate = WindowUpdate;
        Init();
    }

    private void Init()
    {
        _windowRect = _config.DefaultWindowRect;
        _windowId = GetHashCode();
    }

    public void Show()
    {
        if (_closed) return;
        _updateEvents.OnGUIEventUnconditional += OnGUIUnconditional;
    }

    private void Close()
    {
        _updateEvents.OnGUIEventUnconditional -= OnGUIUnconditional;
        _closed = true;
    }

    private void Minimize()
    {
        _updateEvents.OnGUIEventUnconditional -= OnGUIUnconditional;
        _windowManager?.Minimize(this);
    }

    private void OnGUIUnconditional()
    {
        _windowRect = GUILayout.Window(_windowId, _windowRect, _windowUpdate, _config.WindowName,
            _config.LayoutOptions);
    }

    private void WindowUpdate(int id)
    {
        HandleDrag();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // close button
        if (_config.ShowCloseButton &&
            UnityEngine.GUI.Button(new(_windowRect.width - CLOSE_BUTTON_SIZE, 0f, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE),
                "x"))
        {
            Close();
        }

        if (_config.ShowMinimizeButton &&
            UnityEngine.GUI.Button(
                new(_windowRect.width - CLOSE_BUTTON_SIZE * 2, 0f, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE), "_"))
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
            _windowRect.x = mousePos.x - _dragOffset.x;
            _windowRect.y = Screen.height - mousePos.y - _dragOffset.y;
            return;
        }

        if (Event.current.type == EventType.MouseUp)
        {
            // stop dragging
            _dragging = false;
        }

        if (Event.current.type != EventType.layout)
        {
            Event.current.Use();
        }
    }

    private void CheckDrag()
    {
        if (_dragging || Event.current.type != EventType.MouseDown) return;

        var mousePos = (Vector2)UnityInput.Current.mousePosition;
        mousePos.y = Screen.height - mousePos.y;

        // are we dragging now?
        if (_windowRect.Contains(mousePos))
        {
            _dragging = true;
            _dragOffset = mousePos - new Vector2(_windowRect.x, _windowRect.y);
        }
    }

    protected abstract void OnGUI();
}