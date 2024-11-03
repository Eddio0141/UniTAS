using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
public class ObjectPickerSearchSettings(
    WindowDependencies windowDependencies,
    ObjectPickerWindow.SearchSettings searchSettings) : Window(windowDependencies,
    new WindowConfig(showByDefault: true))
{
    private string _filterComponentsText = string.Empty;
    private bool _filterObjsInvalid;

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