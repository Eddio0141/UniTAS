using System;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IUnityMallocTracker
{
    void Malloc(IntPtr ptr, Allocator allocator, bool tracked);
    void Free(IntPtr ptr, Allocator allocator, bool tracked);
}