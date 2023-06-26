using BepInEx;
using UniTAS.Patcher.Services.EventSubscribers;
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
    private Rect _dragAreaRect;
    private int _windowId;
    private readonly UnityEngine.GUI.WindowFunction _windowUpdate;
    private bool _dragging;
    private Vector2 _dragOffset;

    protected abstract Rect DefaultWindowRect { get; }
    private readonly string _windowName;

    private readonly IUpdateEvents _updateEvents;

    private const int DRAG_AREA_HEIGHT = 20;

    private readonly Texture2D _dragAreaTexture;
    private readonly Texture2D _windowBackground;

    protected Window(IUpdateEvents updateEvents, string windowName = null)
    {
        _updateEvents = updateEvents;
        _windowName = windowName ?? string.Empty;

        _dragAreaTexture = new(1, 1)
        {
            wrapMode = TextureWrapMode.Repeat
        };
        _dragAreaTexture.SetPixel(0, 0, new(0.3f, 0.3f, 0.3f));
        _dragAreaTexture.Apply();

        _windowBackground = new(1, 1)
        {
            wrapMode = TextureWrapMode.Repeat
        };
        _windowBackground.SetPixel(0, 0, new(0.2f, 0.2f, 0.2f));
        _windowBackground.Apply();

        _windowUpdate = WindowUpdate;

        Init();
    }

    private void Init()
    {
        _windowRect = DefaultWindowRect;
        _dragAreaRect = new(0, 0, _windowRect.width, DRAG_AREA_HEIGHT);

        _windowId = GetHashCode();
    }

    public void Show()
    {
        _updateEvents.OnGUIEventUnconditional += OnGUIUnconditional;
    }

    private void Close()
    {
        _updateEvents.OnGUIEventUnconditional -= OnGUIUnconditional;
    }

    private readonly GUILayoutOption[] _windowNameOptions = { GUILayout.Height(DRAG_AREA_HEIGHT) };

    private readonly GUIStyle _windowNameStyle = new()
    {
        alignment = TextAnchor.MiddleCenter,
        fontSize = 12,
        fontStyle = FontStyle.Bold
    };

    private readonly GUILayoutOption[] _closeButtonOptions =
        { GUILayout.Width(20), GUILayout.Height(DRAG_AREA_HEIGHT) };

    private void OnGUIUnconditional()
    {
        _windowRect = GUILayout.Window(_windowId, _windowRect, _windowUpdate, _windowName, GUIUtils.EmptyOptions);
    }

    private void WindowUpdate(int id)
    {
        HandleDrag();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // close button
        if (GUILayout.Button("x", _closeButtonOptions))
        {
            Close();
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