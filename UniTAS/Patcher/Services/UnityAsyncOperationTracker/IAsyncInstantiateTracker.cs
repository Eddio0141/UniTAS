using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncInstantiateTracker
{
    object Initialize(Object original, int count, ReadOnlySpan<Vector3> positions, ReadOnlySpan<Quaternion> rotations,
        object parameters, object cancellationToken);
}