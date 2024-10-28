using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
public class ObjectPickerWindow : Window
{
    private readonly IUpdateEvents _updateEvents;
    private readonly IUnityInputWrapper _unityInput;
    private bool _canClickSelect;

    public ObjectPickerWindow(WindowDependencies windowDependencies, IUnityInputWrapper unityInput) : base(
        windowDependencies,
        config: new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(500, 500), windowName: "Object picker"))
    {
        _unityInput = unityInput;
        _updateEvents = windowDependencies.UpdateEvents;
        _raycastCamera = Camera.main;
        _canClickSelect = _raycastCamera != null;
    }

    private Vector2 _scrollPos = Vector2.zero;
    private GameObject[] _objects;
    private readonly List<GameObject> _childFilter = new();

    public override bool Show
    {
        set
        {
            if (_selected && value) return;
            if (value)
            {
                _objects ??= ObjectUtils.FindObjectsOfType<GameObject>();
            }
            else
            {
                _selected = true;
                OnObjectSelected?.Invoke(this, null);
            }

            base.Show = value;
        }
    }

    /// <summary>
    /// Filters by attached children objects
    /// </summary>
    public void AddRangeChildFilter(IEnumerable<GameObject> obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        obj = obj.Where(x => !_childFilter.Contains(x));
        _childFilter.AddRange(obj);
        ApplyFilterToObjects();
    }

    private void ApplyFilterToObjects()
    {
        _objects = _objects.Where(x =>
        {
            var t = x.transform;
            for (var i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i).gameObject;
                foreach (var filterObj in _childFilter)
                {
                    if (child == filterObj) return true;
                }
            }

            return false;
        }).ToArray();
    }

    private Camera _raycastCamera;
    private string _clickSelectText;

    private void ClickSelectUpdate()
    {
        var mousePos = _unityInput.MousePosition;
        var raycastHit = RaycastFromCamera(_raycastCamera, mousePos);

        var builder = new StringBuilder();
        builder.AppendLine("[Left click to select]");
        builder.AppendLine("[Any key to cancel]");
        builder.AppendLine("Selected: ");

        if (raycastHit != null)
        {
            builder.Append(
                $"name: {raycastHit.name} - pos: {raycastHit.position} - rot: {raycastHit.rotation.eulerAngles}");
        }

        _clickSelectText = builder.ToString();

        var close = false;
        if (_unityInput.GetMouseButtonDown(0))
        {
            close = true;
            _objects = [raycastHit.gameObject];
        }

        if (!close && _unityInput.AnyKeyDown)
        {
            close = true;
        }

        if (!close) return;
        _clickSelect = false;
        _updateEvents.OnUpdateUnconditional -= ClickSelectUpdate;
    }

    private void ClickSelectGUIUpdate()
    {
        var currentEvent = Event.current;
        if (!_clickSelect && currentEvent.type == EventType.Layout)
        {
            _updateEvents.OnGUIUnconditional -= ClickSelectGUIUpdate;
            return;
        }

        _clickSelectFontSize ??= UnityEngine.GUI.skin.label.fontSize;

        var mousePos = currentEvent.mousePosition;
        mousePos.x += 25;
        mousePos.y += 25;
        GUIUtils.ShadowedText(_clickSelectText, _clickSelectFontSize.Value, (int)mousePos.x, (int)mousePos.y);
    }

    private int? _clickSelectFontSize;

    private static Transform RaycastFromCamera(Camera camera, Vector2 mousePos)
    {
        if (camera == null) return null;
        var ray = camera.ScreenPointToRay(mousePos);
        return Physics.Raycast(ray, out var hit, 1000f) ? hit.transform : null;
    }

    private bool _clickSelect;

    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (_canClickSelect && GUILayout.Button("Click select"))
        {
            // switch mode to click and select
            _updateEvents.OnUpdateUnconditional += ClickSelectUpdate;
            _updateEvents.OnGUIUnconditional += ClickSelectGUIUpdate;
            _clickSelect = true;
        }

        if (GUILayout.Button("Refresh"))
        {
            // refresh everything
            _objects = ObjectUtils.FindObjectsOfType<GameObject>();
            ApplyFilterToObjects();

            _raycastCamera = Camera.main;
            _canClickSelect = _raycastCamera == null;
        }

        GUILayout.EndHorizontal();

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        foreach (var obj in _objects)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(obj.name))
            {
                OnObjectSelected?.Invoke(this, obj);
                _selected = true;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        if (_selected)
        {
            Show = false;
        }
    }

    public event Action<ObjectPickerWindow, GameObject> OnObjectSelected;
    private bool _selected;
}