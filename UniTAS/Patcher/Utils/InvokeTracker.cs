using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.Invoker;

namespace UniTAS.Patcher.Utils;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class InvokeTracker
{
    private static bool _invoked;

    public static void OnUnityInit()
    {
        if (_invoked) return;
        _invoked = true;

        StaticLogger.Log.LogDebug($"Unity has been initialized, entry at {new StackTrace()}");

        InvokeEventAttributes.Invoke<InvokeOnUnityInitAttribute>();
    }
}