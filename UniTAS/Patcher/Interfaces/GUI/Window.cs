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
    private Vector2 _dragOffset;
    private bool _dragging;

    protected abstract Rect DefaultWindowRect { get; }
    private readonly string _windowName;

    private readonly IUpdateEvents _updateEvents;

    private const int DRAG_AREA_HEIGHT = 20;

    private readonly Texture2D _dragAreaTexture;
    private readonly Texture2D _windowBackground;

    private static bool _alreadyDragging;

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

        Init();
    }

    private void Init()
    {
        _windowRect = DefaultWindowRect;
        _dragAreaRect = new(0, 0, _windowRect.width, DRAG_AREA_HEIGHT);
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

    public void OnGUIUnconditional()
    {
        HandleDrag();

        // window bg
        UnityEngine.GUI.DrawTexture(_windowRect, _windowBackground);

        GUILayout.BeginArea(_windowRect);
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        // drag area
        UnityEngine.GUI.DrawTexture(_dragAreaRect, _dragAreaTexture, ScaleMode.StretchToFill);

        // this needs to be here
        if (!_alreadyDragging && Event.current.type == EventType.MouseDown &&
            _dragAreaRect.Contains(Event.current.mousePosition))
        {
            _dragOffset = (Vector2)UnityInput.Current.mousePosition -
                          new Vector2(_windowRect.x, Screen.height - _windowRect.y);
            _dragging = true;
            _alreadyDragging = true;
        }

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // window name
        GUILayout.Label(_windowName, _windowNameStyle, _windowNameOptions);

        // close button
        if (GUILayout.Button("x", _closeButtonOptions))
        {
            Close();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        OnGUI();

        GUILayout.EndArea();
    }

    private void HandleDrag()
    {
        // stupid hack to make window dragging smooth
        if (_dragging && Event.current.type == EventType.Repaint)
        {
            var mousePos = (Vector2)UnityInput.Current.mousePosition;
            _windowRect.x = mousePos.x - _dragOffset.x;
            _windowRect.y = Screen.height - mousePos.y + _dragOffset.y;
        }

        // drag area events
        if (_dragging && Event.current.type == EventType.MouseUp)
        {
            _dragging = false;
            _alreadyDragging = false;
        }
    }

    protected abstract void OnGUI();
}