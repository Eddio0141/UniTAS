using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

public class CameraWrapper : UnityInstanceWrap
{
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    public CameraWrapper(object instance, IPatchReverseInvoker patchReverseInvoker) : base(instance)
    {
        _patchReverseInvoker = patchReverseInvoker;
        var getScreenRect = AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.rect));
        var setScreenRect = AccessTools.PropertySetter(typeof(Camera), nameof(Camera.rect));
        // var getScreenRectReverse = patchReverseInvoker.RecursiveReversePatch(getScreenRect);
        // var setScreenRectReverse = patchReverseInvoker.RecursiveReversePatch(setScreenRect);
        _getScreenRect = AccessTools.MethodDelegate<Func<Rect>>(getScreenRect, instance);
        _setScreenRect = AccessTools.MethodDelegate<Action<Rect>>(setScreenRect, instance);
    }

    private readonly Func<Rect> _getScreenRect;
    private readonly Action<Rect> _setScreenRect;

    protected override Type WrappedType => typeof(Camera);

    public Rect Rect
    {
        get => _patchReverseInvoker.Invoke(() => _getScreenRect());
        set => _patchReverseInvoker.Invoke(() => _setScreenRect(value));
    }
}