using System;
using System.Reflection;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class MallocTracker : ITryFreeMalloc, IUnityMallocTracker
{
    public void TryFree(object instance, FieldInfo fieldInfo) => TryFreeMallocManual.TryFree(instance, fieldInfo);

    public void Malloc(IntPtr ptr, Allocator allocator, bool tracked) =>
        TryFreeMallocManual.Malloc(ptr, allocator, tracked);

    public void Free(IntPtr ptr, Allocator allocator, bool tracked) =>
        TryFreeMallocManual.Free(ptr, allocator, tracked);
}