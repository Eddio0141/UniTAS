#if !UNIT_TESTS
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using Resolution = UnityEngine.Resolution;
using System;
#endif

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

public struct ResolutionWrapper
{
#if !UNIT_TESTS
    public ResolutionWrapper(int width, int height, RefreshRateWrap rr)
    {
        _instance.width = width;
        _instance.height = height;
        RefreshRateWrap = rr;
    }

    public ResolutionWrapper(Resolution resolution)
    {
        _instance = resolution;
        _instance.width = resolution.width;
        _instance.height = resolution.height;
        _refreshRateWrap = GetRefreshRateRatio == null
            ? new RefreshRateWrap(null) { Rate = _instance.refreshRate }
            : new RefreshRateWrap(GetRefreshRateRatio.Invoke(_instance, null));
    }
    
    public Resolution Instance => _instance;
    private Resolution _instance = new();

    private static readonly MethodInfo GetRefreshRateRatio;

    private static readonly SetRefreshRateRatioDelegate SetRefreshRateRatio;

    private delegate void SetRefreshRateRatioDelegate(ref Resolution resolution, object value);

    static ResolutionWrapper()
    {
        GetRefreshRateRatio = AccessTools.PropertyGetter(typeof(Resolution), "refreshRateRatio");
        var setRefreshRateRatio = AccessTools.PropertySetter(typeof(Resolution), "refreshRateRatio");
        if (setRefreshRateRatio == null) return;
        SetRefreshRateRatio = setRefreshRateRatio.MethodDelegate<SetRefreshRateRatioDelegate>(delegateArgs:
            [typeof(Resolution).MakeByRefType(), typeof(object)]);
    }

    public int Height
    {
        get => _instance.height;
        set => _instance.height = value;
    }

    public int Width
    {
        get => _instance.width;
        set => _instance.width = value;
    }

    private RefreshRateWrap _refreshRateWrap;

    public RefreshRateWrap RefreshRateWrap
    {
        get => _refreshRateWrap;
        set
        {
            _refreshRateWrap = value;
            if (SetRefreshRateRatio == null)
            {
                _instance.refreshRate = (int)Math.Round(value.Rate);
                return;
            }

            SetRefreshRateRatio(ref _instance, value.Instance);
        }
    }
#else
    // ReSharper disable once ConvertToPrimaryConstructor
    public ResolutionWrapper(int width, int height, RefreshRateWrap rr)
    {
        Width = width;
        Height = height;
        RefreshRateWrap = rr;
    }

    public int Height { get; set; }
    public int Width { get; set; }
    public RefreshRateWrap RefreshRateWrap { get; set; }
#endif
}