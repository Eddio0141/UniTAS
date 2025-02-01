using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices;

public static class CoroutineManagerManual
{
    private static ICoroutineTracker _coroutineTracker;

    private static ICoroutineTracker CoroutineTracker
    {
        get
        {
            _coroutineTracker ??= ContainerStarter.Kernel.GetInstance<ICoroutineTracker>();
            return _coroutineTracker;
        }
    }

    public static void MonoBehNewCoroutine(object instance, IEnumerator routine) =>
        CoroutineTracker.NewCoroutine(instance, routine);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static bool CoroutineMoveNextPrefix(IEnumerator __instance, ref bool __result, ref bool __state) =>
        CoroutineTracker.CoroutineMoveNextPrefix(__instance, ref __result, ref __state);
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void CoroutineMoveNextPostfix(IEnumerator __instance, bool __state) =>
        CoroutineTracker.CoroutineMoveNextPostfix(__instance, __state);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void CoroutineCurrentPostfix(IEnumerator __instance, ref object __result) =>
        CoroutineTracker.CoroutineCurrentPostfix(__instance, ref __result);
}