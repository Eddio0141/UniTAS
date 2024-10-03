using System.Collections.Generic;
using BepInEx;
using UniTAS.Patcher.Exceptions.GUI;
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
    private Rect _windowRect;
    private int _windowId;
    private readonly UnityEngine.GUI.WindowFunction _windowUpdate;
    private bool _dragging;
    private bool _resizing;
    private Vector2 _windowClickOffset;
    private Vector2 _resizeSize;
    private bool _showWindow;

    private readonly WindowConfig _config;

    private const int CLOSE_BUTTON_SIZE = 20;

    private readonly IUpdateEvents _updateEvents;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IConfig _configService;

    private string WindowName { get; }
    private readonly string _windowConfigId;

    private static readonly List<string> UsedWindowIDs = new();

    protected Window(WindowDependencies windowDependencies, WindowConfig config, string windowId = null)
    {
        if (windowId != null)
        {
            if (UsedWindowIDs.Contains(windowId))
            {
                throw new DuplicateWindowIDException($"WindowID {windowId} is already used");
            }

            UsedWindowIDs.Add(windowId);
            _windowConfigId = windowId;
        }

        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        _updateEvents = windowDependencies.UpdateEvents;
        _configService = windowDependencies.Config;
        _config = config ?? new();
        _windowUpdate = WindowUpdate;
        WindowName = _config.WindowName;
        Init();
    }

    private Vector2 MousePosition => _patchReverseInvoker.Invoke(() => UnityInput.Current.mousePosition);
    private int ScreenWidth => _patchReverseInvoker.Invoke(() => Screen.width);
    private int ScreenHeight => _patchReverseInvoker.Invoke(() => Screen.height);
    private bool LeftMouseButton => _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButton(0));
    private bool RightMouseButton => _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButton(1));

    private void Init()
    {
        _windowRect = _config.DefaultWindowRect;
        _windowId = GetHashCode();

        // try load config stuff
        if (_configService.TryGetBackendEntry($"{BACKEND_CONFIG_PREFIX}{BACKEND_CONFIG_WINDOW_RECT}{_windowConfigId}",
                out Models.Rect rect))
        {
            _windowRect = rect.ToUnityRect();
        }
    }

    public void Show()
    {
        if (_showWindow) return;
        _updateEvents.OnGUIUnconditional += OnGUIUnconditional;
        _showWindow = true;
    }

    protected virtual void Close()
    {
        _updateEvents.OnGUIUnconditional -= OnGUIUnconditional;
        _showWindow = false;
    }

    private void OnGUIUnconditional()
    {
        _windowRect = GUILayout.Window(_windowId, _windowRect, _windowUpdate, WindowName, _config.LayoutOptions);
    }

    private void WindowUpdate(int id)
    {
        HandleDragResize();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // close button
        if (UnityEngine.GUI.Button(new(_windowRect.width - CLOSE_BUTTON_SIZE, 0f, CLOSE_BUTTON_SIZE, CLOSE_BUTTON_SIZE),
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

            _windowRect.width = _resizeSize.x + (mousePos.x - _windowRect.x - _windowClickOffset.x);
            _windowRect.height = _resizeSize.y + (ScreenHeight - mousePos.y - _windowRect.y - _windowClickOffset.y);

            // just in case
            if (!RightMouseButton)
            {
                _resizing = false;
                ClampWindow(); // handle clamped pos
                SaveWindowRect();
            }
            else
            {
                ClampWindow();
            }

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
            _windowRect.x = mousePos.x - _windowClickOffset.x;
            _windowRect.y = ScreenHeight - mousePos.y - _windowClickOffset.y;

            // just in case
            if (!LeftMouseButton)
            {
                _dragging = false;
                ClampWindow(); // for saving clamped pos
                SaveWindowRect();
            }
            else
            {
                ClampWindow();
            }

            return;
        }

        if (Event.current.type != EventType.layout)
        {
            Event.current.Use();
        }
    }

    private void ClampWindow()
    {
        if (_windowRect.x < 0)
        {
            _windowRect.x = 0;
        }

        if (_windowRect.y < 0)
        {
            _windowRect.y = 0;
        }

        var screenWidth = ScreenWidth;
        if (screenWidth < _windowRect.width)
        {
            _windowRect.width = screenWidth;
        }

        if (_windowRect.xMax > screenWidth)
        {
            _windowRect.x = screenWidth - _windowRect.width;
        }

        var screenHeight = ScreenHeight;
        if (screenHeight < _windowRect.height)
        {
            _windowRect.height = screenHeight;
        }

        if (_windowRect.yMax > screenHeight)
        {
            _windowRect.y = screenHeight - _windowRect.height;
        }
    }

    private void CheckDragResize()
    {
        if (Event.current.type != EventType.MouseDown) return;

        if (!_resizing && Event.current.button == 1)
        {
            // are we resizing now?
            var mousePos = MousePosition;
            mousePos.y = ScreenHeight - mousePos.y;

            if (_windowRect.Contains(mousePos))
            {
                _resizing = true;
                _windowClickOffset = mousePos - new Vector2(_windowRect.x, _windowRect.y);
                _resizeSize = new(_windowRect.width, _windowRect.height);

                return;
            }
        }

        if (_dragging || Event.current.button != 0) return;

        var mousePos2 = MousePosition;
        mousePos2.y = ScreenHeight - mousePos2.y;

        // are we dragging now?
        if (_windowRect.Contains(mousePos2))
        {
            _dragging = true;
            _windowClickOffset = mousePos2 - new Vector2(_windowRect.x, _windowRect.y);
        }
    }

    protected abstract void OnGUI();

    private const string BACKEND_CONFIG_PREFIX = "window-";
    private const string BACKEND_CONFIG_WINDOW_RECT = "rect-";

    private void SaveWindowRect()
    {
        var saneRect = new Models.Rect(_windowRect);
        _configService.WriteBackendEntry($"{BACKEND_CONFIG_PREFIX}{BACKEND_CONFIG_WINDOW_RECT}{_windowConfigId}",
            saneRect);
    }
}