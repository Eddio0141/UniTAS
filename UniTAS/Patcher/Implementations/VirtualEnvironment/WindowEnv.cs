using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton]
[ExcludeRegisterIfTesting]
public class WindowEnv(
    ILogger logger,
    IPatchReverseInvoker patchReverseInvoker,
    IUnityInstanceWrapFactory unityInstanceWrapFactory) : IWindowEnv, IOnVirtualEnvStatusChange, IScreenTrackerUpdate
{
    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (runVirtualEnv)
        {
            var currentRes = patchReverseInvoker.Invoke(() => Screen.currentResolution);
            _originalResolution = unityInstanceWrapFactory.Create<IResolutionWrapper>(currentRes);
            _originalIsFullScreen = GetFullScreenMode == null
                ? patchReverseInvoker.Invoke(() => Screen.fullScreen)
                : unityInstanceWrapFactory
                    .Create<FullScreenModeWrap>(patchReverseInvoker.Invoke(() => GetFullScreenMode.Invoke(null, null)))
                    .FullScreen;

            var useFullScreen = currentRes.width == CurrentResolution.Width &&
                                currentRes.height == CurrentResolution.Height;
            logger.LogDebug(
                $"setting game resolution to {CurrentResolution.Width}x{CurrentResolution.Height}, use full screen: {useFullScreen}");

            patchReverseInvoker.Invoke(() =>
                Screen.SetResolution(CurrentResolution.Width, CurrentResolution.Height, useFullScreen));
            return;
        }

        logger.LogDebug(
            $"restoring original resolution {_originalResolution.Width}x{_originalResolution.Height}, full screen: {_originalIsFullScreen}");

        patchReverseInvoker.Invoke(() =>
            Screen.SetResolution(_originalResolution.Width, _originalResolution.Height, _originalIsFullScreen));
    }

    public IResolutionWrapper CurrentResolution { get; set; } = new ResolutionWrapper(Screen.currentResolution);

    public IResolutionWrapper[] ExtraSupportedResolutions { get; set; } = [];
    public void AddExtraSupportedResolutions(IResolutionWrapper[] extraSupportedResolutions)
    {
    }

    public void ClearExtraSupportedResolutions()
    {
    }

    public bool FullScreen { get; set; } = Screen.fullScreen;

    public FullScreenModeWrap FullScreenMode { get; set; } = GetFullScreenModeMethod == null
        ? null
        : patchReverseInvoker.Invoke(() =>
            unityInstanceWrapFactory.Create<FullScreenModeWrap>(GetFullScreenModeMethod.Invoke(null, null)));

    private static readonly MethodInfo GetFullScreenModeMethod =
        AccessTools.PropertyGetter(typeof(Screen), "fullScreenMode");

    private IResolutionWrapper _originalResolution;
    private bool _originalIsFullScreen;

    public void SetResolution(int width, int height, bool fullScreen)
    {
        _originalResolution.Width = width;
        _originalResolution.Height = height;
        _originalIsFullScreen = fullScreen;
    }

    public void SetResolution(int width, int height, bool fullScreen, int refreshRate)
    {
        _originalResolution.Width = width;
        _originalResolution.Height = height;
        _originalResolution.RefreshRateWrap.Rate = refreshRate;
        _originalIsFullScreen = fullScreen;
    }

    public void SetResolution(int width, int height, object fullScreenMode, int refreshRate)
    {
        _originalResolution.Width = width;
        _originalResolution.Height = height;
        _originalResolution.RefreshRateWrap.Rate = refreshRate;
        _originalIsFullScreen = unityInstanceWrapFactory.Create<FullScreenModeWrap>(fullScreenMode).FullScreen;
    }

    public void SetResolution(int width, int height, object fullScreenMode, object refreshRate)
    {
        _originalResolution.Width = width;
        _originalResolution.Height = height;
        _originalResolution.RefreshRateWrap = unityInstanceWrapFactory.Create<RefreshRateWrap>(refreshRate);
        _originalIsFullScreen = unityInstanceWrapFactory.Create<FullScreenModeWrap>(fullScreenMode).FullScreen;
    }

    private static readonly MethodInfo GetFullScreenMode;

    static WindowEnv()
    {
        var fullScreenModeType = AccessTools.TypeByName("UnityEngine.FullScreenMode");
        if (fullScreenModeType == null) return;

        GetFullScreenMode = AccessTools.PropertyGetter(typeof(Screen), "fullScreenMode");
    }
}