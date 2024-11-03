using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
public class ObjectPickerWindow : Window
{
    public ObjectPickerWindow(WindowDependencies windowDependencies, IUnityInputWrapper unityInput,
        IWindowFactory windowFactory) : base(
        windowDependencies,
        config: new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(500, 500), windowName: "Object picker"))
    {
        _unityInput = unityInput;
        _windowFactory = windowFactory;
        _updateEvents = windowDependencies.UpdateEvents;
        var objectIconTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        windowDependencies.TextureWrapper.LoadImage(objectIconTexture,
            Path.Combine(UniTASPaths.Resources, "object-icon.png"));
        _objNameContent = new(objectIconTexture);

        var objExpandable = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        windowDependencies.TextureWrapper.LoadImage(objExpandable,
            Path.Combine(UniTASPaths.Resources, "object-expandable.png"));
        _objExpandable = new(objExpandable);

        var objClosable = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        windowDependencies.TextureWrapper.LoadImage(objClosable,
            Path.Combine(UniTASPaths.Resources, "object-closable.png"));
        _objClosable = new(objClosable);
    }

    private readonly IUpdateEvents _updateEvents;
    private readonly IWindowFactory _windowFactory;

    private Vector2 _scrollPos = Vector2.zero;

    // layer and object, 0 being the root
    private List<ObjectData> _objects;
    private List<ObjectData> _objectsBeforeSearch;

    private struct ObjectData
    {
        public int Depth;
        public GameObject Object;
        public bool Folded;
    }

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
            .Select(g => new ObjectData { Depth = 0, Object = g, Folded = true }).OrderBy(o => o.Object.name).ToList();
        for (var i = 0; i < _objects.Count; i++)
        {
            var objInfo = _objects[i];
            var children = GrabChildrenRecursive(objInfo.Object, 1).ToArray();
            _objects.InsertRange(i + 1, children);
            i += children.Length;
        }

        ApplyFilterToObjects();
    }

    private static IEnumerable<ObjectData> GrabChildrenRecursive(GameObject parent, int depth)
    {
        var parentTransform = parent.transform;
        var childCount = parentTransform.childCount;
        for (var i = 0; i < childCount; i++)
        {
            var child = parentTransform.GetChild(i);
            var go = child.gameObject;
            yield return new ObjectData { Depth = depth, Object = go, Folded = true };
            foreach (var foundRecursive in GrabChildrenRecursive(go, depth + 1))
            {
                yield return foundRecursive;
            }
        }
    }

    private static readonly Func<GameObject, bool> ActiveInHierarchy;

    private void ApplyFilterToObjects()
    {
        if (_searchSettings.Active.HasValue)
        {
            var objs = _objects.Where(o => o.Object != null);
            objs = ActiveInHierarchy == null
                ? objs.Where(o => o.Object.active == _searchSettings.Active.Value)
                : objs.Where(o => ActiveInHierarchy(o.Object) == _searchSettings.Active.Value);
            _objects = objs.ToList();
        }

        if (_searchSettings.FilterComponents.Count == 0) return;

        // filter out objs by component type
        var objsBefore = _objects.Where(o => o.Object != null).ToList();
        _objects.Clear();

        var addedIndexes = new bool[objsBefore.Count];
        for (var i = 0; i < objsBefore.Count; i++)
        {
            var objInfo = objsBefore[i];

            // component match?
            if (!_searchSettings.FilterComponents.Any(c => objInfo.Object.GetComponent(c))) continue;

            var insertIndex = _objects.Count;
            addedIndexes[i] = true;
            objInfo.Folded = false;
            _objects.Add(objInfo);

            if (objInfo.Depth == 0) continue;

            // also add objects backwards till reaching parent
            for (var j = i - 1; j >= 0; j--)
            {
                var prev = objsBefore[j];
                if (prev.Depth >= objInfo.Depth) continue;
                if (addedIndexes[j]) continue;

                _objects.Insert(insertIndex, prev);
                addedIndexes[j] = true;
                prev.Folded = false;

                if (prev.Depth == 0) break;
            }
        }
    }

    private Camera _raycastCamera;
    private string _clickSelectText;

    private void ClickSelectUpdate()
    {
        var mousePos = _unityInput.MousePosition;
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
        if (_unityInput.GetMouseButtonDown(0))
        {
            close = true;
            _objects = [new ObjectData { Depth = 0, Object = raycastHit.gameObject, Folded = true }];
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
    private string _search;
    private GUIStyle _objNameStyle;

    private readonly GUIContent _objNameContent;
    private readonly GUIContent _objExpandable;
    private readonly GUIContent _objClosable;

    private bool _settingsOpen;
    private SearchSettings _searchSettings = new();

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

            if (!_search.IsNullOrWhiteSpace())
            {
                _objectsBeforeSearch = _objects.ToList();
                FilterBySearch();
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Search settings") && !_settingsOpen)
        {
            _settingsOpen = true;
            var searchSettings = _windowFactory.Create(_searchSettings);
            searchSettings.OnSearchSettingsComplete += newSettings =>
            {
                if (newSettings != null)
                {
                    _searchSettings = newSettings;
                    RefreshObjects();
                    ApplyFilterToObjects();
                }

                _settingsOpen = false;
            };
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
                _objects = _objectsBeforeSearch.ToList();
                _objectsBeforeSearch = null;
            }
            else
            {
                _objectsBeforeSearch ??= _objects.ToList();
                FilterBySearch();
            }
        }

        GUILayout.EndHorizontal();

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        _objects.RemoveAll(x => x.Object == null);

        var objsCount = _scrollViewElementHeight == null ? 1 : _objects.Count;
        var dummyEntryCount = 0;
        var beforeVisible = true;
        var afterVisible = false;
        var iActual = -1;

        var collapseDepths = new Stack<int>();

        for (var i = 0; i < objsCount; i++)
        {
            var objData = _objects[i];

            if (collapseDepths.Count > 0 && objData.Depth > collapseDepths.Peek())
                continue;

            while (collapseDepths.Count > 0)
            {
                // any pop?
                if (collapseDepths.Peek() >= objData.Depth)
                    collapseDepths.Pop();

                break;
            }

            if (objData.Folded)
                collapseDepths.Push(objData.Depth);

            iActual++;

            // is this entry out of view
            if (_scrollViewElementHeight.HasValue)
            {
                if (beforeVisible)
                {
                    var entryY = iActual * _scrollViewElementHeight.Value;

                    if (entryY + _scrollViewElementHeight.Value >= _scrollPos.y)
                    {
                        // now the elements are visible, put the big ass spacing before rendering new stuff
                        if (dummyEntryCount > 0)
                        {
                            GUILayout.Space(dummyEntryCount * _scrollViewElementHeight.Value);
                            dummyEntryCount = 0;
                        }

                        beforeVisible = false;
                    }
                    else
                    {
                        dummyEntryCount++;
                        continue;
                    }
                }
                else if (afterVisible)
                {
                    // current in after visible section, just count
                    dummyEntryCount++;
                    continue;
                }
                else
                {
                    var entryY = iActual * _scrollViewElementHeight.Value;

                    // visible
                    if (entryY > _scrollViewHeight.Value + _scrollPos.y)
                    {
                        // sike not anymore
                        dummyEntryCount++;
                        afterVisible = true;
                    }
                }
            }

            GUILayout.BeginHorizontal();
            RenderObjectEntry(ref objData, i + 1 < _objects.Count ? _objects[i + 1] : null);
            _objects[i] = objData;
            GUILayout.EndHorizontal();

            if (i == 0 && _scrollViewElementHeight == null && Event.current.type == EventType.Repaint)
            {
                var scrollViewLineRect = GUILayoutUtility.GetLastRect();
                _scrollViewElementHeight = scrollViewLineRect.height;
            }
        }

        // now fill with remaining space
        if (dummyEntryCount > 0 && _scrollViewElementHeight.HasValue && afterVisible)
        {
            GUILayout.Space(_scrollViewElementHeight.Value * dummyEntryCount);
        }

        GUILayout.EndScrollView();

        if (_scrollViewHeight == null && Event.current.type == EventType.Repaint)
        {
            var scrollViewLineRect = GUILayoutUtility.GetLastRect();
            _scrollViewHeight = scrollViewLineRect.height;
        }

        GUILayout.EndVertical();

        if (_selected)
        {
            Show = false;
        }
    }

    private void RenderObjectEntry(ref ObjectData objData, ObjectData? nextObjData)
    {
        if (objData.Depth > 0)
            GUILayout.Space(objData.Depth * _objFoldIconXEnd.GetValueOrDefault(1));
        _objNameStyle ??= new GUIStyle(UnityEngine.GUI.skin.label) { alignment = TextAnchor.MiddleLeft };

        // can we fold?
        if (nextObjData.HasValue && nextObjData.Value.Depth > objData.Depth)
        {
            var foldTexture = objData.Folded ? _objExpandable : _objClosable;
            if (GUILayout.Button(foldTexture, _objNameStyle, GUILayout.ExpandWidth(false)))
            {
                objData.Folded = !objData.Folded;
            }

            if (_objFoldIconXEnd == null && Event.current.type == EventType.Repaint)
            {
                _objFoldIconXEnd = GUILayoutUtility.GetLastRect().xMax + _objNameStyle.margin.left;
            }
        }

        _objNameContent.text = $" {objData.Object.name}";
        if (GUILayout.Button(_objNameContent, _objNameStyle))
        {
            OnObjectSelected?.Invoke(this, objData.Object);
            _selected = true;
        }
    }

    private float? _scrollViewElementHeight;
    private float? _scrollViewHeight;

    private float? _objFoldIconXEnd;

    private void FilterBySearch()
    {
        _objects.Clear();

        var addedIndexes = new bool[_objectsBeforeSearch.Count];
        for (var i = 0; i < _objectsBeforeSearch.Count; i++)
        {
            var objData = _objectsBeforeSearch[i];
            if (objData.Object == null) continue;
            if (!objData.Object.name.ToLowerInvariant().Contains(_search)) continue;
            objData.Folded = false;

            var objInsertIndex = _objects.Count;
            _objects.Add(objData);
            addedIndexes[i] = true;

            // traverse till depth 0
            var targetDepth = objData.Depth;
            if (targetDepth == 0) continue;

            for (var j = i - 1; j >= 0; j--)
            {
                var objPrev = _objectsBeforeSearch[j];
                if (objPrev.Depth >= targetDepth)
                    continue;

                targetDepth = objPrev.Depth;
                if (addedIndexes[j]) continue;
                objPrev.Folded = false;
                _objects.Insert(objInsertIndex, objPrev);
                addedIndexes[j] = true;

                if (objPrev.Depth == 0) break;
            }
        }
    }

    public event Action<ObjectPickerWindow, GameObject> OnObjectSelected;
    private bool _selected;
    private readonly IUnityInputWrapper _unityInput;

    public class SearchSettings
    {
        public bool? Active;
        public readonly List<Type> FilterComponents = new();
    }

    static ObjectPickerWindow()
    {
        var activeInHierarchy = AccessTools.PropertyGetter(typeof(GameObject), "activeInHierarchy");
        if (activeInHierarchy != null)
            ActiveInHierarchy = AccessTools.MethodDelegate<Func<GameObject, bool>>(activeInHierarchy);
    }
}