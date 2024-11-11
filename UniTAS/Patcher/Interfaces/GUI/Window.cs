using System;
using System.IO;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.NoRefresh;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
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
            OnShowChange?.Invoke(this, _show);

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

            if (!_show && _config.RemoveConfigOnClose)
            {
                _configService.RemoveBackendEntry(_backendConfigWindowRect);
                _configService.RemoveBackendEntry(_backendConfigWindowShown);
            }
            else
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
            Resizable = false;
        }
    }

    private const int CloseButtonSize = 20;

    private readonly IUpdateEvents _updateEvents;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IConfig _configService;
    private readonly IToolBar _toolBar;
    private readonly INoRefresh _noRefresh;
    private readonly ITextureWrapper _textureWrapper;
    private readonly IUnityInputWrapper _unityInputWrapper;

    private string WindowName { get; set; }
    public string WindowConfigId { get; }
    private WindowConfig _config = new();

    protected bool NoWindowDuringToolBarHide { get; set; }
    private bool _windowShownToolbarHide;
    protected bool Resizable { get; set; }

    protected Window(WindowDependencies windowDependencies, string windowId = null)
    {
        WindowConfigId = windowId;
        InitConfigNames();

        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        _updateEvents = windowDependencies.UpdateEvents;
        _configService = windowDependencies.Config;
        _toolBar = windowDependencies.ToolBar;
        _noRefresh = windowDependencies.NoRefresh;
        _textureWrapper = windowDependencies.TextureWrapper;
        _unityInputWrapper = windowDependencies.UnityInputWrapper;
    }

    protected Window(WindowDependencies windowDependencies, WindowConfig config, string windowId = null)
    {
        WindowConfigId = windowId;
        InitConfigNames();

        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;
        _updateEvents = windowDependencies.UpdateEvents;
        _configService = windowDependencies.Config;
        _toolBar = windowDependencies.ToolBar;
        _noRefresh = windowDependencies.NoRefresh;
        _textureWrapper = windowDependencies.TextureWrapper;
        _unityInputWrapper = windowDependencies.UnityInputWrapper;
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

    private Vector2 MousePosition =>
        _patchReverseInvoker.Invoke(wrapper => wrapper.GetMousePosition(), _unityInputWrapper);

    private int ScreenWidth => _patchReverseInvoker.Invoke(() => Screen.width);
    private int ScreenHeight => _patchReverseInvoker.Invoke(() => Screen.height);

    private bool LeftMouseButton =>
        _patchReverseInvoker.Invoke(wrapper => wrapper.GetMouseButton(0), _unityInputWrapper);

    private bool RightMouseButton =>
        _patchReverseInvoker.Invoke(wrapper => wrapper.GetMouseButton(1), _unityInputWrapper);

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

    private static GUIStyle _windowStyle;
    private static readonly Texture2D WindowBgOnNormal = new Texture2D(1, 1, TextureFormat.ARGB32, false);
    private static readonly Texture2D WindowBgNormal = new Texture2D(1, 1, TextureFormat.ARGB32, false);

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

        if (_windowStyle == null)
        {
            _windowStyle = new GUIStyle(UnityEngine.GUI.skin.window);
            _textureWrapper.LoadImage(WindowBgOnNormal, Path.Combine(UniTASPaths.Resources, "window-on-normal.png"));
            _textureWrapper.LoadImage(WindowBgNormal, Path.Combine(UniTASPaths.Resources, "window-normal.png"));
            _windowStyle.onNormal.background = WindowBgOnNormal;
            _windowStyle.normal.background = WindowBgNormal;
        }

        _windowRect = GUILayout.Window(_windowId, _windowRect, WindowUpdate, WindowName, _windowStyle,
            Config.LayoutOptions);
    }

    private static GUIStyle _closeButtonStyle;
    private static readonly Texture2D CloseButtonNormal = new Texture2D(1, 1, TextureFormat.ARGB32, false);
    private static readonly Texture2D CloseButtonHover = new Texture2D(1, 1, TextureFormat.ARGB32, false);
    private static readonly Texture2D CloseButtonClick = new Texture2D(1, 1, TextureFormat.ARGB32, false);

    private void WindowUpdate(int id)
    {
        HandleDragResize();

        if (_toolBar.Show || !NoWindowDuringToolBarHide)
        {
            GUILayout.BeginVertical(GUIUtils.EmptyOptions);
            GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

            // close button
            if (_closeButtonStyle == null)
            {
                _closeButtonStyle = new GUIStyle(UnityEngine.GUI.skin.button);

                _textureWrapper.LoadImage(CloseButtonNormal,
                    Path.Combine(UniTASPaths.Resources, "window-close-normal.png"));
                _textureWrapper.LoadImage(CloseButtonHover,
                    Path.Combine(UniTASPaths.Resources, "window-close-hover.png"));
                _textureWrapper.LoadImage(CloseButtonClick,
                    Path.Combine(UniTASPaths.Resources, "window-close-click.png"));
                _closeButtonStyle.normal.background = CloseButtonNormal;
                _closeButtonStyle.hover.background = CloseButtonHover;
                _closeButtonStyle.active.background = CloseButtonClick;
            }

            if (_config.CloseButtonShow && UnityEngine.GUI.Button(
                    new(_windowRect.width - CloseButtonSize, 0f, CloseButtonSize, CloseButtonSize),
                    GUIContent.none, _closeButtonStyle))
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
    public event Action<Window, bool> OnShowChange;

    /// <summary>
    /// Updates window size based on last element's size
    /// </summary>
    /// <returns>True if the function was able to resize</returns>
    protected bool FitWindowSize()
    {
        if (Event.current.type != EventType.Repaint)
            return false;

        var rect = GUILayoutUtility.GetLastRect();
        var windowRect = WindowRect;
        windowRect.width = rect.width;
        windowRect.height = rect.height;
        WindowRect = windowRect;
        return true;
    }
}