using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.NoRefresh;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class NoRefresh : INoRefresh, IUpdateCameraInfo, IOverridingCameraInfo, IOnSceneLoad
{
    private readonly bool _canNoRefresh;

    private const string CONFIG_ENTRY_NAME = "NoRefreshCache";

    private readonly IUnityInstanceWrapFactory _unityInstanceWrapFactory;

    public NoRefresh(IConfig config, IUnityInstanceWrapFactory unityInstanceWrapFactory, ILogger logger,
        IPatchReverseInvoker patchReverseInvoker)
    {
        _unityInstanceWrapFactory = unityInstanceWrapFactory;

        if (!UniTASSha256Info.InvalidCache)
        {
            if (config.TryGetBackendEntry(CONFIG_ENTRY_NAME, out NoRefreshCacheConfig entry))
            {
                _canNoRefresh = entry.CanNoRefresh;
                LogCanRefresh(logger);
                return;
            }
        }

        // find if we can no-refresh
        _canNoRefresh = AccessTools.AllTypes().Any(x => !x.IsAbstract && x.IsSubclassOf(typeof(MonoBehaviour)) &&
                                                        (x.GetMethod("OnBecameVisible",
                                                             BindingFlags.Instance | BindingFlags.Public |
                                                             BindingFlags.NonPublic) != null ||
                                                         x.GetMethod("OnBecameInvisible",
                                                             BindingFlags.Instance | BindingFlags.Public |
                                                             BindingFlags.NonPublic) != null));

        LogCanRefresh(logger);

        var cache = new NoRefreshCacheConfig(_canNoRefresh);
        config.WriteBackendEntry(CONFIG_ENTRY_NAME, cache);
    }

    private void LogCanRefresh(ILogger logger)
    {
        logger.LogInfo(_canNoRefresh ? "no refresh is available" : "no refresh isn't available");
    }

    private class NoRefreshCacheConfig(bool canNoRefresh)
    {
        public bool CanNoRefresh { get; } = canNoRefresh;
    }

    private bool _enabled;

    private readonly List<(CameraWrapper, Rect)> _cameras = new();

    public bool Enable
    {
        get => _enabled;
        set
        {
            if (!_canNoRefresh) return;
            if (_enabled == value) return;
            _enabled = value;

            if (value)
            {
                AddCameras();
                return;
            }

            foreach (var (cam, rect) in _cameras)
            {
                cam.Rect = rect;
            }

            _cameras.Clear();
        }
    }

    public bool SetRect(Camera camera, Rect rect)
    {
        if (camera == null) return false;
        var camIndex = _cameras.FindIndex(x => ReferenceEquals(x.Item1.Instance, camera));
        if (camIndex < 0) return false;
        var camTuple = _cameras[camIndex];
        camTuple.Item2 = rect;
        _cameras[camIndex] = camTuple;
        return true;
    }

    public bool GetRect(Camera camera, out Rect rect)
    {
        var camIndex = _cameras.FindIndex(x => ReferenceEquals(x.Item1.Instance, camera));
        if (camIndex < 0)
        {
            rect = default;
            return false;
        }

        rect = _cameras[camIndex].Item2;
        return true;
    }

    public void OnSceneLoad()
    {
        // cameras are probably dead
        for (var i = 0; i < _cameras.Count; i++)
        {
            if (_cameras[i].Item1.Instance != null) continue;

            _cameras.RemoveAt(i);
            i--;
        }

        // bring in the cameras
        AddCameras();
    }

    private void AddCameras()
    {
        var cams = ObjectUtils.FindObjectsOfType<Camera>();

        _cameras.AddRange(cams.Select(cam =>
        {
            var wrap = _unityInstanceWrapFactory.Create<CameraWrapper>(cam);
            return (wrap, wrap.Rect);
        }));

        foreach (var (cam, _) in _cameras)
        {
            cam.Rect = new(0, 0, 0, 0);
        }
    }
}