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

    private const string ConfigEntryName = "NoRefreshCache";

    private readonly IUnityInstanceWrapFactory _unityInstanceWrapFactory;
    private readonly ILogger _logger;

    public NoRefresh(IConfig config, IUnityInstanceWrapFactory unityInstanceWrapFactory, ILogger logger,
        IPatchReverseInvoker patchReverseInvoker)
    {
        _unityInstanceWrapFactory = unityInstanceWrapFactory;
        _logger = logger;

        if (!UniTASSha256Info.GameCacheInvalid && !UniTASSha256Info.UniTASInvalidCache)
        {
            if (config.TryGetBackendEntry(ConfigEntryName, out NoRefreshCacheConfig entry))
            {
                _canNoRefresh = entry.CanNoRefresh;
                logger.LogDebug("using cached no-refresh search result");
                LogCanRefresh(logger);
                return;
            }
        }

        // find if we can no-refresh
        _canNoRefresh = !AccessTools.AllTypes().Any(x => !x.IsAbstract && x.IsSubclassOf(typeof(MonoBehaviour)) &&
                                                         (x.GetMethod("OnBecameVisible",
                                                              BindingFlags.Instance | BindingFlags.Public |
                                                              BindingFlags.NonPublic) != null ||
                                                          x.GetMethod("OnBecameInvisible",
                                                              BindingFlags.Instance | BindingFlags.Public |
                                                              BindingFlags.NonPublic) != null));

        LogCanRefresh(logger);

        var cache = new NoRefreshCacheConfig(_canNoRefresh);
        config.WriteBackendEntry(ConfigEntryName, cache);
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
                _logger.LogInfo("enabling no refresh");
                AddCameras();
                return;
            }

            _logger.LogDebug("disabling no refresh");

            foreach (var (cam, rect) in _cameras)
            {
                if ((Object)cam.Instance == null) continue;
                cam.Rect = rect;
            }

            _cameras.Clear();
        }
    }

    public bool SetRect(Camera camera, Rect rect)
    {
        if (!_enabled || camera == null) return false;
        var camIndex = _cameras.FindIndex(x => ReferenceEquals(x.Item1.Instance, camera));
        if (camIndex < 0) return false;
        var camTuple = _cameras[camIndex];
        camTuple.Item2 = rect;
        _cameras[camIndex] = camTuple;
        return true;
    }

    public bool GetRect(Camera camera, out Rect rect)
    {
        if (!_enabled || camera == null)
        {
            rect = default;
            return false;
        }

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
        if (!_enabled) return;

        // cameras are probably dead
        _cameras.RemoveAll(x => (Camera)x.Item1.Instance == null);

        // bring in the cameras
        AddCameras();
    }

    private void AddCameras()
    {
        var cams = ObjectUtils.FindObjectsOfType<Camera>();

        foreach (var cam in cams)
        {
            if (_cameras.Any(x => (Camera)x.Item1.Instance == cam)) continue;
            var wrap = _unityInstanceWrapFactory.Create<CameraWrapper>(cam);
            _cameras.Add((wrap, cam.rect));
            wrap.Rect = new();
        }

        _logger.LogDebug($"no refresh: tracking {_cameras.Count} cameras");
    }
}