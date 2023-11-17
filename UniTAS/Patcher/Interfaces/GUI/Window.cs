using BepInEx;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Interfaces.GUI;

/// <summary>
/// Base class for all windows.
/// Uses automatic layout.
/// </summary>
public abstract class Window
{
    private readonly IUpdateEvents _updateEvents;
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    protected Rect WindowRect { get; set; }
    private int _windowId;
    private readonly UnityEngine.GUI.WindowFunction _windowUpdate;
    private bool _dragging;
    private bool _resizing;
    private Vector2 _windowClickOffset;
    private Vector2 _resizeSize;
    private bool _showWindow;

    private readonly WindowConfig _config;

    private const int CLOSE_BUTTON_SIZE = 20;

    private string _windowName;
    private GUIStyle _style;

    protected Window(WindowDependencies windowDependencies, WindowConfig config)
    {
        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        _updateEvents = windowDependencies.UpdateEvents;
        _config = config;
        _windowUpdate = WindowUpdate;
        Init();
        _updateEvents.OnGUIUnconditional += GrabStyle;
    }

    private Vector2 MousePosition => _patchReverseInvoker.Invoke(() => UnityInput.Current.mousePosition);
    private int ScreenWidth => _patchReverseInvoker.Invoke(() => Screen.width);
    private int ScreenHeight => _patchReverseInvoker.Invoke(() => Screen.height);
    private bool LeftMouseButton => _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButton(0));
    private bool RightMouseButton => _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButton(1));

    private void Init()
    {
        _windowId = GetHashCode();
        WindowRect = _config.DefaultWindowRect;
        _windowName = _config.WindowName;
    }

    public void Show()
    {
        if (_showWindow) return;
        _updateEvents.OnGUIUnconditional += OnGUIUnconditional;
        _showWindow = true;
    }

    public virtual void Close()
    {
        if (!_showWindow) return;
        _updateEvents.OnGUIUnconditional -= OnGUIUnconditional;
        _showWindow = false;
    }

    private void GrabStyle()
    {
        _updateEvents.OnGUIUnconditional -= GrabStyle;
        var skin = UnityEngine.GUI.skin;
        if (skin == null || skin.window == null)
        {
            _style ??= new();
            return;
        }

        _style ??= skin.window;
    }

    private void OnGUIUnconditional()
    {
        WindowRect = GUILayout.Window(_windowId, WindowRect, _windowUpdate, _windowName, _style,
            _config.LayoutOptions);
    }

    private void WindowUpdate(int id)
    {
        HandleDragResize();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // close button
        if (UnityEngine.GUI.Button(new(WindowRect.width - CLOSE_BUTTON_SIZE, 0f, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE),
                "x"))
        {
            Close();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        OnGUI();

        CheckDragResize();
    }

    private void HandleDragResize()
    {
        if (_resizing)
        {
            HandleResize();
            return;
        }

        if (_dragging)
        {
            HandleDrag();
        }
    }

    private void HandleResize()
    {
        // handle resize
        if (Event.current.type == EventType.Repaint)
        {
            var mousePos = MousePosition;

            var window = WindowRect;
            window.width = _resizeSize.x + (mousePos.x - window.x - _windowClickOffset.x);
            window.height = _resizeSize.y + (ScreenHeight - mousePos.y - window.y - _windowClickOffset.y);
            WindowRect = window;

            // just in case
            if (!RightMouseButton)
            {
                _resizing = false;
            }

            ClampWindow();

            return;
        }

        if (Event.current.type != EventType.layout)
        {
            Event.current.Use();
        }
    }

    private void HandleDrag()
    {
        // stupid hack to make window dragging smooth
        if (Event.current.type == EventType.Repaint)
        {
            var mousePos = MousePosition;
            var window = WindowRect;
            var newRect = new Rect(mousePos.x - _windowClickOffset.x, ScreenHeight - mousePos.y - _windowClickOffset.y,
                window.width, window.height);
            WindowRect = newRect;

            // just in case
            if (!LeftMouseButton)
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
        var window = WindowRect;
        if (window.x < 0)
        {
            window.x = 0;
        }

        if (window.y < 0)
        {
            window.y = 0;
        }

        var screenWidth = ScreenWidth;
        if (screenWidth < window.width)
        {
            window.width = screenWidth;
        }

        if (window.xMax > screenWidth)
        {
            window.x = screenWidth - window.width;
        }

        var screenHeight = ScreenHeight;
        if (screenHeight < window.height)
        {
            window.height = screenHeight;
        }

        if (window.yMax > screenHeight)
        {
            window.y = screenHeight - window.height;
        }

        WindowRect = window;
    }

    private void CheckDragResize()
    {
        if (Event.current.type != EventType.MouseDown) return;

        if (!_resizing && Event.current.button == 1)
        {
            // are we resizing now?
            var mousePos = MousePosition;
            mousePos.y = ScreenHeight - mousePos.y;

            if (WindowRect.Contains(mousePos))
            {
                _resizing = true;
                _windowClickOffset = mousePos - new Vector2(WindowRect.x, WindowRect.y);
                _resizeSize = new(WindowRect.width, WindowRect.height);

                return;
            }
        }

        if (_dragging || Event.current.button != 0) return;

        var mousePos2 = MousePosition;
        mousePos2.y = ScreenHeight - mousePos2.y;

        // are we dragging now?
        if (WindowRect.Contains(mousePos2))
        {
            _dragging = true;
            _windowClickOffset = mousePos2 - new Vector2(WindowRect.x, WindowRect.y);
        }
    }

    protected abstract void OnGUI();
}