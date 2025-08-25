using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices.Trackers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ObjectAsyncInitTrackerManual
{
    private static IAsyncInstantiateTracker _asyncInstantiateTracker;

    public static bool Instantiate(object original, int count, ReadOnlySpan<Vector3Alt> positions,
        ReadOnlySpan<QuaternionAlt> rotations, object parameters, object cancellationToken, ref object __result)
    {
        _asyncInstantiateTracker ??= ContainerStarter.Kernel.GetInstance<IAsyncInstantiateTracker>();
        return _asyncInstantiateTracker.Instantiate(original, count, positions, rotations, parameters,
            cancellationToken,
            ref __result);
    }

    public static bool Instantiate(object original, int count, object parent, ReadOnlySpan<Vector3Alt> positions,
        ReadOnlySpan<QuaternionAlt> rotations, ref object __result)
    {
        _asyncInstantiateTracker ??= ContainerStarter.Kernel.GetInstance<IAsyncInstantiateTracker>();
        return _asyncInstantiateTracker.Instantiate(original, count, positions, rotations, null, null, ref __result);
    }
}