using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using Resolution = UnityEngine.Resolution;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

[Register]
public class ResolutionWrapper : UnityInstanceWrap, IResolutionWrapper
{
    private static readonly MethodInfo GetRefreshRateRatio;
    private static readonly MethodInfo SetRefreshRateRatio;

    static ResolutionWrapper()
    {
        GetRefreshRateRatio = AccessTools.PropertyGetter(typeof(Resolution), "refreshRateRatio");
        SetRefreshRateRatio = AccessTools.PropertySetter(typeof(Resolution), "refreshRateRatio");
    }

    public int Height
    {
        get => ((Resolution)Instance).height;
        set
        {
            var resolution = (Resolution)Instance;
            resolution.height = value;
            Instance = resolution;
        }
    }

    public int Width
    {
        get => ((Resolution)Instance).width;
        set
        {
            var resolution = (Resolution)Instance;
            resolution.width = value;
            Instance = resolution;
        }
    }

    private RefreshRateWrap _refreshRateWrap;

    public ResolutionWrapper(object instance) : base(instance)
    {
        _refreshRateWrap = GetRefreshRateRatio == null
            ? new RefreshRateWrap(null) { Rate = ((Resolution)Instance).refreshRate }
            : new RefreshRateWrap(GetRefreshRateRatio.Invoke((Resolution)Instance, []));
    }

    public RefreshRateWrap RefreshRateWrap
    {
        get => _refreshRateWrap;
        set
        {
            _refreshRateWrap = value;
            if (SetRefreshRateRatio == null)
            {
                var instance = (Resolution)Instance;
                instance.refreshRate = (int)Math.Round(value.Rate);
                Instance = instance;
                return;
            }

            SetRefreshRateRatio.Invoke(Instance, [value.Instance]);
        }
    }

    protected override Type WrappedType => typeof(Resolution);
}