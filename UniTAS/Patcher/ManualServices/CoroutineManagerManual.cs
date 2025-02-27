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
    public static bool CoroutineMoveNextPrefix(IEnumerator __instance, ref bool __result) =>
        CoroutineTracker.CoroutineMoveNextPrefix(__instance, ref __result);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void CoroutineCurrentPostfix(IEnumerator __instance, ref object __result) =>
        CoroutineTracker.CoroutineCurrentPostfix(__instance, ref __result);
}