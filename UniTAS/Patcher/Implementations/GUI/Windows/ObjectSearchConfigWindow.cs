using System;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
public class ObjectSearchConfigWindow(
    WindowDependencies windowDependencies,
    UnityObjectIdentifier.SearchSettings searchSettings)
    : Window(windowDependencies,
        new WindowConfig(windowName: "Object detection options", showByDefault: true))
{
    private UnityObjectIdentifier.SearchSettings _searchSettings = searchSettings;

    private static readonly string[] IDSearchTypes = Enum.GetValues(typeof(UnityObjectIdentifier.IdSearchType))
        .Cast<UnityObjectIdentifier.IdSearchType>().Select(x => x.ToString()).ToArray();

    private static readonly string[] MultipleMatchHandleTypes =
        Enum.GetValues(typeof(UnityObjectIdentifier.MultipleMatchHandle))
            .Cast<UnityObjectIdentifier.MultipleMatchHandle>().Select(x => x.ToString()).ToArray();

    private bool _done;

    public override bool Show
    {
        set
        {
            if (value && _done) return;
            if (!value) _done = true;
            base.Show = value;
        }
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Label("ID search type");
        var searchType = GUILayout.Toolbar((int)_searchSettings.IdSearchType, IDSearchTypes);
        _searchSettings.IdSearchType = (UnityObjectIdentifier.IdSearchType)searchType;

        GUILayout.Label("Multiple match handler");
        var multipleMatchHandleType =
            GUILayout.Toolbar((int)_searchSettings.MultipleMatchHandle, MultipleMatchHandleTypes);
        _searchSettings.MultipleMatchHandle = (UnityObjectIdentifier.MultipleMatchHandle)multipleMatchHandleType;
        GUILayout.Space(5);

        _searchSettings.NameMatch = GUILayout.Toggle(_searchSettings.NameMatch, "Match by name");
        _searchSettings.ParentMatch = GUILayout.Toggle(_searchSettings.ParentMatch, "Match by parent");
        _searchSettings.ComponentMatch = GUILayout.Toggle(_searchSettings.ComponentMatch, "Match by components");
        _searchSettings.SceneMatch = GUILayout.Toggle(_searchSettings.SceneMatch, "Match by scene");

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Confirm"))
        {
            OnSearchSettingsChanged?.Invoke(this, _searchSettings);
            Show = false;
        }

        if (GUILayout.Button("Cancel"))
        {
            OnSearchSettingsChanged?.Invoke(this, null);
            Show = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        if (!_initialResize && FitWindowSize())
            _initialResize = true;
    }

    private bool _initialResize;

    public event Action<ObjectSearchConfigWindow, UnityObjectIdentifier.SearchSettings?> OnSearchSettingsChanged;
}