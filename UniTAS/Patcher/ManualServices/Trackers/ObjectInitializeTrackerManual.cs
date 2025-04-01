using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices.Trackers;

public class ObjectInitializeTrackerManual
{
    private static IAsyncInstantiateTracker _asyncInstantiateTracker;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static bool Initialize(object original, int count, object positions, object rotations, object parameters,
        object cancellationToken, ref object __result)
    {
        _asyncInstantiateTracker ??= ContainerStarter.Kernel.GetInstance<IAsyncInstantiateTracker>();
        return _asyncInstantiateTracker.Initialize(original, count, positions, rotations, parameters, cancellationToken,
            ref __result);
    }
}