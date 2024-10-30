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
public class ObjectPickerWindow(WindowDependencies windowDependencies, IUnityInputWrapper unityInput)
    : Window(windowDependencies,
        config: new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(500, 500), windowName: "Object picker"))
{
    private readonly IUpdateEvents _updateEvents = windowDependencies.UpdateEvents;

    private Vector2 _scrollPos = Vector2.zero;

    // layer and object, 0 being the root
    private List<(int, GameObject)> _objects;
    private List<(int, GameObject)> _objectsBeforeSearch;
    private readonly List<GameObject> _childFilter = new();

    public override bool Show
    {
        set
        {
            if (_selected && value) return;
            if (value)
            {
                RefreshObjects();
            }
            else
            {
                _selected = true;
                OnObjectSelected?.Invoke(this, null);
            }

            base.Show = value;
        }
    }

    private void RefreshObjects()
    {
        _objects = ObjectUtils.FindObjectsOfType<GameObject>().Where(g => g.transform.parent == null)
            .Select(g => (0, g)).ToList();
        for (var i = 0; i < _objects.Count; i++)
        {
            var (_, go) = _objects[i];
            var children = GrabChildrenRecursive(go, 1).ToArray();
            _objects.InsertRange(i + 1, children);
            i += children.Length;
        }

        ApplyFilterToObjects();
    }

    private static IEnumerable<(int, GameObject)> GrabChildrenRecursive(GameObject parent, int depth)
    {
        var parentTransform = parent.transform;
        var childCount = parentTransform.childCount;
        for (var i = 0; i < childCount; i++)
        {
            var child = parentTransform.GetChild(i);
            var go = child.gameObject;
            yield return (depth, go);
            foreach (var foundRecursive in GrabChildrenRecursive(go, depth + 1))
            {
                yield return foundRecursive;
            }
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
        if (_childFilter.Count == 0) return;

        _objects = _objects.Where(tupleArg =>
        {
            var (_, x) = tupleArg;
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
        }).ToList();
    }

    private Camera _raycastCamera;
    private string _clickSelectText;

    private void ClickSelectUpdate()
    {
        var mousePos = unityInput.MousePosition;
        if (_raycastCamera == null)
        {
            _raycastCamera = Camera.main;
        }

        if (_raycastCamera == null)
        {
            _clickSelect = false;
            _updateEvents.OnUpdateUnconditional -= ClickSelectUpdate;
        }

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
        if (unityInput.GetMouseButtonDown(0))
        {
            close = true;
            _objects = [(0, raycastHit.gameObject)];
        }

        if (!close && unityInput.AnyKeyDown)
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
    private string _search;
    private GUIStyle _objNameStyle;

    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Click select"))
        {
            _raycastCamera = Camera.main;
            if (_raycastCamera != null)
            {
                // switch mode to click and select
                _updateEvents.OnUpdateUnconditional += ClickSelectUpdate;
                _updateEvents.OnGUIUnconditional += ClickSelectGUIUpdate;
                _clickSelect = true;
            }
        }

        if (GUILayout.Button("Refresh"))
        {
            // refresh everything
            RefreshObjects();

            if (_search != null)
            {
                _objectsBeforeSearch = _objects;
                _objects = FilterBySearch(_objectsBeforeSearch).ToList();
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Search: ", GUILayout.ExpandWidth(false));
        var newSearch = GUILayout.TextField(_search, GUILayout.ExpandWidth(true));
        if (newSearch != _search)
        {
            _search = newSearch.ToLowerInvariant();

            if (_search.Length == 0 && _objectsBeforeSearch != null)
            {
                _objects = _objectsBeforeSearch;
                _objectsBeforeSearch = null;
                _search = null;
            }
            else
            {
                _objectsBeforeSearch ??= _objects;
                _objects = FilterBySearch(_objectsBeforeSearch).ToList();
            }
        }

        GUILayout.EndHorizontal();

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        foreach (var (depth, obj) in _objects)
        {
            if (obj == null) continue;

            GUILayout.BeginHorizontal();

            if (depth > 0)
                GUILayout.Space(depth * 20);
            _objNameStyle ??= new GUIStyle(UnityEngine.GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            if (GUILayout.Button(obj.name, _objNameStyle))
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

    private IEnumerable<(int, GameObject)> FilterBySearch(IEnumerable<(int, GameObject)> objects)
    {
        return objects.Where(x => x.Item2 != null && x.Item2.name.ToLowerInvariant().Contains(_search));
    }

    public event Action<ObjectPickerWindow, GameObject> OnObjectSelected;
    private bool _selected;
}