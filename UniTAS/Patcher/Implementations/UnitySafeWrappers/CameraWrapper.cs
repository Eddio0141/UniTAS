using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

public class CameraWrapper(object instance, IPatchReverseInvoker patchReverseInvoker) : UnityInstanceWrap(instance)
{
    private static readonly Func<Camera, Rect> GetScreenRect;
    private static readonly Action<Camera, Rect> SetScreenRect;
    // private static readonly MethodInfo _setScreenRect;

    static CameraWrapper()
    {
        var getScreenRect = AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.rect));
        var setScreenRect = AccessTools.PropertySetter(typeof(Camera), nameof(Camera.rect));
        // var getScreenRectReverse = patchReverseInvoker.RecursiveReversePatch(getScreenRect);
        // var setScreenRectReverse = patchReverseInvoker.RecursiveReversePatch(setScreenRect);
        GetScreenRect = AccessTools.MethodDelegate<Func<Camera, Rect>>(getScreenRect);
        SetScreenRect = AccessTools.MethodDelegate<Action<Camera, Rect>>(setScreenRect);
    }

    protected override Type WrappedType => typeof(Camera);

    public Rect Rect
    {
        get => patchReverseInvoker.Invoke(() => GetScreenRect((Camera)Instance));
        set => patchReverseInvoker.Invoke(() => SetScreenRect((Camera)Instance, value));
    }
}