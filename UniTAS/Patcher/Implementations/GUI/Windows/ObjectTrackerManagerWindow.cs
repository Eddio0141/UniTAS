using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
[ForceInstantiate]
public class ObjectTrackerManagerWindow : Window
{
    private bool _addPicker;
    private readonly List<UnityObjectIdentifier> _trackers;
    private readonly IConfig _config;
    private readonly IWindowFactory _windowFactory;
    private readonly IUnityObjectIdentifierFactory _objectIdentifierFactory;

    public ObjectTrackerManagerWindow(WindowDependencies windowDependencies, IWindowFactory windowFactory,
        IUnityObjectIdentifierFactory objectIdentifierFactory) : base(
        windowDependencies,
        new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(500, 500), windowName: "Object trackers"),
        "Object tracker manager")
    {
        _windowFactory = windowFactory;
        _objectIdentifierFactory = objectIdentifierFactory;
        _config = windowDependencies.Config;

        if (_config.TryGetBackendEntry(TrackerConfigSave, out _trackers))
        {
            foreach (var tracker in _trackers)
            {
                NewTrackerWindow(tracker);
            }

            return;
        }

        _trackers = new();
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add") && !_addPicker)
        {
            // modes to pick object:
            // - from camera attached (list picker but filtered)
            // - from screen (click on it)
            // - from list
            var picker = _windowFactory.Create<ObjectPickerWindow>();
            _addPicker = true;
            picker.OnObjectSelected += ObjectSelected;
            var cams = ObjectUtils.FindObjectsOfType<Camera>().Select(x => x.gameObject);
            picker.Show = true;
            picker.AddRangeChildFilter(cams);
        }

        if (GUILayout.Button("Remove"))
        {
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    // when object is picked for tracking
    private void ObjectSelected(ObjectPickerWindow sender, Object o)
    {
        _addPicker = false;
        sender.OnObjectSelected -= ObjectSelected;
        if (o == null) return;
        var newTracker = _objectIdentifierFactory.NewUnityObjectIdentifier(o);
        if (_trackers.Contains(newTracker)) return;
        _trackers.Add(newTracker);
        SaveTrackers();
        NewTrackerWindow(newTracker);
    }

    private const string TrackerConfigSave = "ObjectTrackers";

    private void SaveTrackers()
    {
        _config.WriteBackendEntry(TrackerConfigSave, _trackers);
    }

    private void NewTrackerWindow(UnityObjectIdentifier identifier)
    {
        _windowFactory.Create(identifier);
    }
}