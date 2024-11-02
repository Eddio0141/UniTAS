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
    private string _filterObjsText = string.Empty;
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

    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        GUILayout.Label("Filter by component type");

        _invalidText ??= new GUIStyle(UnityEngine.GUI.skin.textArea) { normal = { textColor = Color.red } };
        var filterObjsStyle = _filterObjsInvalid ? _invalidText : UnityEngine.GUI.skin.textArea;

        var newStr = GUILayout.TextArea(_filterObjsText, filterObjsStyle, GUILayout.MinWidth(250),
            GUILayout.MinHeight(100));
        if (newStr != _filterObjsText)
        {
            // whatever idc parse them all
            _filterObjsInvalid = false;
            _filterObjsText = newStr;
            searchSettings.FilterComponents.Clear();
            foreach (var objTypeName in _filterObjsText.Split(['\n'], StringSplitOptions.RemoveEmptyEntries))
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

                searchSettings.FilterComponents.Add(objType);
            }
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        UnityEngine.GUI.enabled = !_filterObjsInvalid;
        if (GUILayout.Button("Done"))
        {
            OnSearchSettingsComplete?.Invoke(searchSettings);
            Show = false;
        }

        UnityEngine.GUI.enabled = true;

        GUILayout.FlexibleSpace();

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