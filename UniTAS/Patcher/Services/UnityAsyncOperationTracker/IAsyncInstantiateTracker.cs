using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncInstantiateTracker
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    bool Initialize(object original, int count, object positions, object rotations, object parameters,
        object cancellationToken, ref object __result);
}