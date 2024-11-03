using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UniTAS.Patcher.Implementations.GUI.Windows;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.UnityEvents;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class ObjectTrackerManager : IObjectTrackerManager
{
    private bool _addPickerOpen;
    private readonly List<UnityObjectIdentifier> _trackers;
    private readonly List<ObjectTrackerInstanceWindow> _trackerWindows = new();

    public ReadOnlyCollection<(UnityObjectIdentifier, ObjectTrackerInstanceWindow)> Trackers =>
        _trackers.Select((t, i) => (t, _trackerWindows[i])).ToList().AsReadOnly();

    private readonly IConfig _config;
    private readonly IWindowFactory _windowFactory;
    private readonly IUnityObjectIdentifierFactory _objectIdentifierFactory;
    private readonly IUpdateEvents _updateEvents;

    public ObjectTrackerManager(IConfig config, IWindowFactory windowFactory,
        IUnityObjectIdentifierFactory objectIdentifierFactory, ICoroutine coroutine, IUpdateEvents updateEvents)
    {
        _windowFactory = windowFactory;
        _objectIdentifierFactory = objectIdentifierFactory;
        _updateEvents = updateEvents;
        _config = config;

        if (!_config.TryGetBackendEntry(TrackerConfigSave, out _trackers))
        {
            _trackers = new();
        }

        // stupid hack to stop dependency loop on constructor phase
        updateEvents.OnFixedUpdateUnconditional += InitTrackerWindows;
    }

    private void InitTrackerWindows()
    {
        foreach (var tracker in _trackers)
        {
            NewTrackerWindow(tracker);
        }

        _updateEvents.OnFixedUpdateUnconditional -= InitTrackerWindows;
    }

    public void AddNew()
    {
        if (_addPickerOpen) return;

        // modes to pick object:
        // - from camera attached (list picker but filtered)
        // - from screen (click on it)
        // - from list
        var picker = _windowFactory.Create<ObjectPickerWindow>();
        _addPickerOpen = true;
        picker.OnObjectSelected += ObjectSelected;
        // var cams = ObjectUtils.FindObjectsOfType<Camera>().Select(x => x.gameObject);
        picker.Show = true;
        // picker.AddRangeChildFilter(cams);
    }

    // when object is picked for tracking
    private void ObjectSelected(ObjectPickerWindow sender, Object o)
    {
        _addPickerOpen = false;
        sender.OnObjectSelected -= ObjectSelected;
        if (o == null) return;
        var newTracker = _objectIdentifierFactory.NewUnityObjectIdentifier(o);
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
        var window = _windowFactory.Create(identifier);
        window.OnShowChange += (_, show) =>
        {
            if (show) return;
            // if its hiding, remove from tracker
            _trackers.Remove(identifier);
            _trackerWindows.Remove(window);
            SaveTrackers();
        };
        _trackerWindows.Add(window);
    }
}