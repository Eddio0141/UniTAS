using System;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.ManualServices.Trackers;

public class ObjectInitializeTrackerManual
{
    private static IAsyncInstantiateTracker _asyncInstantiateTracker;

    public static object Initialize(Object original, int count, ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<Quaternion> rotations, object parameters, object cancellationToken)
    {
        _asyncInstantiateTracker ??= ContainerStarter.Kernel.GetInstance<IAsyncInstantiateTracker>();
        return _asyncInstantiateTracker.Initialize(original, count, positions, rotations, parameters, cancellationToken);
    }
}