using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.Invoker;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class InvokeTracker
{
    private static bool _invoked;

    public static void OnUnityInit()
    {
        if (_invoked) return;
        if (!IsUnityActuallyInit()) return;
        _invoked = true;

        StaticLogger.Log.LogDebug("Unity has been initialized");

        InvokeEventAttributes.Invoke<InvokeOnUnityInitAttribute>();
    }

    private static bool IsUnityActuallyInit()
    {
        // stupid test
        try
        {
            Time.captureFramerate = 0;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}