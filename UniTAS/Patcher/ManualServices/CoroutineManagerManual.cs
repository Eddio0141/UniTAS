using System.Collections;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.ManualServices;

public static class CoroutineManagerManual
{
    private static ICoroutineTracker _coroutineTracker;

    public static void MonoBehNewCoroutine(MonoBehaviour instance, IEnumerator routine)
    {
        _coroutineTracker ??= ContainerStarter.Kernel.GetInstance<ICoroutineTracker>();
        _coroutineTracker.NewCoroutine(instance, routine);
    }
}