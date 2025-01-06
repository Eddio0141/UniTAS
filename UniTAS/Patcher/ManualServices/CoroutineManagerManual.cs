using System.Collections;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices;

public static class CoroutineManagerManual
{
    private static ICoroutineTracker _coroutineTracker;

    public static void MonoBehNewCoroutine(object instance, IEnumerator routine)
    {
        _coroutineTracker ??= ContainerStarter.Kernel.GetInstance<ICoroutineTracker>();
        _coroutineTracker.NewCoroutine(instance, routine);
    }
}