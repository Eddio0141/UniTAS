using System.Collections;
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

    public static bool CoroutineMoveNextPrefix(IEnumerator instance, ref bool result) =>
        CoroutineTracker.CoroutineMoveNextPrefix(instance, ref result);

    public static void CoroutineCurrentPostfix(IEnumerator instance, ref object result) =>
        CoroutineTracker.CoroutineCurrentPostfix(instance, ref result);
}