using System;
using System.Collections.Generic;
using BepInEx;
using UniTAS.Patcher.Exceptions.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
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

    public Rect WindowRect
    {
        get => _windowRect;
        set => _windowRect = value;
    }

    private int _windowId;
    private bool _dragging;
    private bool _resizing;
    private Vector2 _windowClickOffset;
    private Vector2 _resizeSize;

    public virtual bool Show
    {
        get => _show;
        set
        {
            if (_show == value) return;
            _show = value;

            if (_show)
            {
                _updateEvents.OnGUIUnconditional += OnGUIUnconditional;
                _toolBar.OnShowChange += ToolBarOnOnShowChange;
                ClampWindow();
            }
            else
            {
                _updateEvents.OnGUIUnconditional -= OnGUIUnconditional;
                _toolBar.OnShowChange -= ToolBarOnOnShowChange;
            }

            SaveWindowShown();

            return;

            void ToolBarOnOnShowChange(bool _) => ClampWindow();
        }
    }

    private bool _initialized;

    protected WindowConfig Config
    {
        get => _config;
        set
        {
            _config = value;
            WindowName = Config.WindowName;
        }
    }

    private const int CloseButtonSize = 20;

    private readonly IUpdateEvents _updateEvents;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IConfig _configService;
    private readonly IToolBar _toolBar;

    private string WindowName { get; set; }
    public string WindowConfigId { get; }
    private WindowConfig _config = new();

    private static readonly List<string> UsedWindowIDs = new();

    protected bool NoWindowDuringToolBarHide { get; set; }
    private bool _windowShownToolbarHide;
    protected bool Resizable { get; set; }

    protected Window(WindowDependencies windowDependencies, string windowId = null)
    {
        WindowConfigId = windowId;
        ValidateWindowId();
        InitConfigNames();

        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        _updateEvents = windowDependencies.UpdateEvents;
        _configService = windowDependencies.Config;
        _toolBar = windowDependencies.ToolBar;
    }

    protected Window(WindowDependencies windowDependencies, WindowConfig config, string windowId = null)
    {
        WindowConfigId = windowId;
        ValidateWindowId();
        InitConfigNames();

        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        _updateEvents = windowDependencies.UpdateEvents;
        _configService = windowDependencies.Config;
        _toolBar = windowDependencies.ToolBar;
        if (config != null)
        {
            Config = config;
        }

        Init();
    }

    private const string BackendConfigPrefix = "window-";

    private string _backendConfigWindowRect;
    private string _backendConfigWindowShown;
    private bool _show;

    private void InitConfigNames()
    {
        if (WindowConfigId == null) return;
        _backendConfigWindowRect = $"{BackendConfigPrefix}rect-{WindowConfigId}";
        _backendConfigWindowShown = $"{BackendConfigPrefix}show-{WindowConfigId}";
    }

    private void ValidateWindowId()
    {
        if (WindowConfigId == null) return;

        if (UsedWindowIDs.Contains(WindowConfigId))
        {
            throw new DuplicateWindowIDException($"WindowID {WindowConfigId} is already used");
        }

        UsedWindowIDs.Add(WindowConfigId);
    }

    private Vector2 MousePosition => _patchReverseInvoker.Invoke(() => UnityInput.Current.mousePosition);
    private int ScreenWidth => _patchReverseInvoker.Invoke(() => Screen.width);
    private int ScreenHeight => _patchReverseInvoker.Invoke(() => Screen.height);
    private bool LeftMouseButton => _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButton(0));
    private bool RightMouseButton => _patchReverseInvoker.Invoke(() => UnityInput.Current.GetMouseButton(1));

    protected void Init()
    {
        if (_initialized) return;
        _initialized = true;

        _windowRect = Config.DefaultWindowRect;
        _windowId = GetHashCode();

        _windowShownToolbarHide = !_toolBar.Show;

        if (WindowConfigId == null)
        {
            if (Config.ShowByDefault)
                Show = true;
            return;
        }

        // try load config stuff
        if (_configService.TryGetBackendEntry(_backendConfigWindowRect, out Models.Rect rect))
        {
            _windowRect = rect.ToUnityRect();
        }

        if (_configService.TryGetBackendEntry(_backendConfigWindowShown, out bool shown))
        {
            if (shown)
                Show = true;
        }
        else if (Config.ShowByDefault)
        {
            Show = true;
        }
    }

    private bool _pendingNewLayout = true;

    private void OnGUIUnconditional()
    {
        var currentEvent = Event.current;

        if (!_toolBar.Show && NoWindowDuringToolBarHide)
        {
            if (!_windowShownToolbarHide)
            {
                _pendingNewLayout = true;
                _windowShownToolbarHide = true;
            }

            if (_pendingNewLayout && currentEvent.type != EventType.Layout)
            {
                return;
            }

            _pendingNewLayout = false;

            GUILayout.BeginArea(_windowRect);
            WindowUpdate(-1);
            GUILayout.EndArea();

            return;
        }

        if (_windowShownToolbarHide)
        {
            _pendingNewLayout = true;
            _windowShownToolbarHide = false;
        }

        if (_pendingNewLayout && currentEvent.type != EventType.Layout)
        {
            return;
        }

        _pendingNewLayout = false;

        _windowRect = GUILayout.Window(_windowId, _windowRect, WindowUpdate, WindowName, Config.LayoutOptions);
    }

    private void WindowUpdate(int id)
    {
        HandleDragResize();

        if (_toolBar.Show || !NoWindowDuringToolBarHide)
        {
            GUILayout.BeginVertical(GUIUtils.EmptyOptions);
            GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

            // close button
            if (UnityEngine.GUI.Button(new(_windowRect.width - CloseButtonSize, 0f, CloseButtonSize, CloseButtonSize),
                    "x"))
            {
                Show = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        if (_toolBar.Show)
        {
            OnGUI();
        }
        else
        {
            OnGUIWhileToolbarHide();
        }

        if (_toolBar.Show || !NoWindowDuringToolBarHide)
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
            if (!RightMouseButton || (!_toolBar.Show && NoWindowDuringToolBarHide) || !Resizable)
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
            if (!LeftMouseButton || (!_toolBar.Show && NoWindowDuringToolBarHide))
            {
                _dragging = false;
                ClampWindow(); // for saving clamped pos
                SaveWindowRect();
                OnDragEnd?.Invoke();
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

        if (Resizable && !_resizing && Event.current.button == 1)
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

    /// <summary>
    /// Function that is called instead when toolbar is hidden
    /// By default, OnGUI is called
    /// </summary>
    protected virtual void OnGUIWhileToolbarHide()
    {
        OnGUI();
    }

    private void SaveWindowRect()
    {
        if (WindowConfigId == null) return;

        var saneRect = new Models.Rect(_windowRect);
        _configService.WriteBackendEntry(_backendConfigWindowRect, saneRect);
    }

    private void SaveWindowShown()
    {
        if (WindowConfigId == null) return;

        _configService.WriteBackendEntry(_backendConfigWindowShown, Show);
    }

    public event Action OnDragEnd;
}