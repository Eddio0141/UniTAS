using System;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UnityEngine;
using Object = UnityEngine.Object;
using Rect = UnityEngine.Rect;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
public class ObjectPickerSearchSettings(
    WindowDependencies windowDependencies,
    ObjectPickerWindow.SearchSettings searchSettings,
    IDropdownList dropdownList,
    IObjectTrackerManager objectTrackerManager) : Window(windowDependencies,
    new WindowConfig(windowName: "Object picker settings"))
{
    private string _filterComponentsText = string.Empty;
    private bool _filterObjsInvalid;
    private bool _sortByTrackerDropDown;
    private (string, Action)[] _sortByTrackerButtons;
    private static Rect? _sortByTrackerRect;

    private bool _completed;

    public override bool Show
    {
        set
        {
            if (_completed) return;
            if (!value)
            {
                _completed = true;
            }

            base.Show = value;
        }
    }

    private GUIStyle _invalidText;
    private ObjectPickerWindow.SearchSettings _searchSettings = searchSettings;

    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.Label("Object active");
        var selectionInt = GUILayout.Toolbar(
            _searchSettings.Active == null ? 0 : (_searchSettings.Active.Value ? 1 : 2),
            ["Ignore", "Active", "Inactive"]);
        _searchSettings.Active = selectionInt switch
        {
            0 => null,
            1 => true,
            2 => false,
            _ => throw new ArgumentOutOfRangeException()
        };

        GUILayout.Label("Filter by component type");

        _invalidText ??= new GUIStyle(UnityEngine.GUI.skin.textArea) { normal = { textColor = Color.red } };
        var filterObjsStyle = _filterObjsInvalid ? _invalidText : UnityEngine.GUI.skin.textArea;

        var newStr = GUILayout.TextArea(_filterComponentsText, filterObjsStyle, GUILayout.MinWidth(250),
            GUILayout.MinHeight(100));
        if (newStr != _filterComponentsText)
        {
            // whatever idc parse them all
            _filterObjsInvalid = false;
            _filterComponentsText = newStr;
            _searchSettings.FilterComponents.Clear();
            foreach (var objTypeName in _filterComponentsText.Split(['\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                var objTypeName2 = objTypeName.Trim();
                if (objTypeName2.Length == 0) continue;
                var objType = AccessTools.TypeByName(objTypeName2);
                if (objType == null)
                {
                    _filterObjsInvalid = true;
                    continue;
                }

                if (!objType.IsSubclassOf(typeof(Object)) && objType != typeof(Object))
                {
                    _filterObjsInvalid = true;
                    continue;
                }

                _searchSettings.FilterComponents.Add(objType);
            }
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Presets");

        if (GUILayout.Button("FPS game - main character"))
        {
            _filterComponentsText = "UnityEngine.Camera\n";
            _searchSettings.FilterComponents.Clear();
            _searchSettings.FilterComponents.Add(typeof(Camera));
            _searchSettings.Active = true;
        }

        if (GUILayout.Button("Sort by tracker distance") && !_sortByTrackerDropDown)
        {
            _sortByTrackerDropDown = true;
            _sortByTrackerButtons = objectTrackerManager.Trackers
                .Select<(UnityObjectIdentifier, ObjectTrackerInstanceWindow), (string, Action)>(tuple => (
                    tuple.Item2.TrackingSettings.Name,
                    () =>
                    {
                        var t = tuple.Item2.Transform;
                        if (t != null)
                            _searchSettings.SortByDist = (tuple.Item2, t.position, t);
                    })).ToArray();
        }

        if (_sortByTrackerDropDown)
        {
            if (_sortByTrackerRect == null && Event.current.type == EventType.Repaint)
            {
                var lastRect = GUILayoutUtility.GetLastRect();
                _sortByTrackerRect =
                    new Rect(lastRect.x, lastRect.y + lastRect.height, lastRect.width, lastRect.height);
            }

            if (_sortByTrackerRect != null)
                if (dropdownList.DropdownButtons(_sortByTrackerRect.Value, _sortByTrackerButtons))
                    _sortByTrackerDropDown = false;
        }

        if (!_sortByTrackerDropDown)
            GUILayout.Label(
                $"Selected: {(_searchSettings.SortByDist == null ? "None" : _searchSettings.SortByDist.Value.Item1.TrackingSettings.Name)}");

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        UnityEngine.GUI.enabled = !_filterObjsInvalid;
        if (GUILayout.Button("Done"))
        {
            OnSearchSettingsComplete?.Invoke(_searchSettings);
            Show = false;
        }

        UnityEngine.GUI.enabled = true;

        if (GUILayout.Button("Reset"))
        {
            _searchSettings = new();
            _filterComponentsText = string.Empty;
        }

        if (GUILayout.Button("Cancel"))
        {
            OnSearchSettingsComplete?.Invoke(null);
            Show = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        FitWindowSize();
    }

    public event Action<ObjectPickerWindow.SearchSettings> OnSearchSettingsComplete;
}