using System;
using System.Diagnostics.CodeAnalysis;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncInstantiateTracker
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    bool Instantiate(object original, int count, ReadOnlySpan<Vector3Alt> positions,
        ReadOnlySpan<QuaternionAlt> rotations, object parameters, object cancellationToken, ref object __result);
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public struct Vector3Alt
{
    public float x;
    public float y;
    public float z;
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public struct QuaternionAlt
{
    public float x;
    public float y;
    public float z;
    public float w;
}