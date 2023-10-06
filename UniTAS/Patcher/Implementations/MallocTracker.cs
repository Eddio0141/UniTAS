using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class MallocTracker : ITryFreeMalloc, IUnityMallocTracker
{
    private readonly List<UnityMallocInfo> _unityMallocs = new();

    private readonly MethodBase _unityFree = AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:Free");

    private readonly MethodBase _unityFreeTracked =
        AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:FreeTracked");

    private readonly Type _allocator = AccessTools.TypeByName("Unity.Collections.Allocator");

    private readonly bool _hasUnityMalloc;

    private readonly ILogger _logger;

    public MallocTracker(ILogger logger)
    {
        _logger = logger;

        _hasUnityMalloc = _unityFree != null && _unityFreeTracked != null && _allocator != null;
    }

    private void TryFree(IntPtr ptr)
    {
        if (_hasUnityMalloc && TryFreeUnityMalloc(ptr)) return;

        _logger.LogWarning($"Could not find malloc at {ptr} to free");
    }

    public void TryFree(object instance, FieldInfo fieldInfo)
    {
        if (!fieldInfo.FieldType.IsPointer) return;

        var prevValue = fieldInfo.GetValue(instance);
        unsafe
        {
            var prevValueRaw = (IntPtr)Pointer.Unbox(prevValue);
            TryFree(prevValueRaw);
        }
    }

    private bool TryFreeUnityMalloc(IntPtr ptr)
    {
        var unityMallocIndex = _unityMallocs.FindIndex(x => x.Ptr == ptr);
        if (unityMallocIndex < 0) return false;

        var unityMalloc = _unityMallocs[unityMallocIndex];

        var allocator = Enum.Parse(_allocator, unityMalloc.Allocator.ToString());
        _logger.LogDebug($"Freeing unity malloc at {ptr} with allocator {allocator}");

        // free
        if (unityMalloc.Tracked)
        {
            _unityFreeTracked.Invoke(null, new[] { ptr, allocator });
        }
        else
        {
            _unityFree.Invoke(null, new[] { ptr, allocator });
        }

        return true;
    }

    public void Malloc(IntPtr ptr, Allocator allocator, bool tracked)
    {
        _unityMallocs.Add(new(ptr, allocator, tracked));
    }

    public void Free(IntPtr ptr, Allocator allocator, bool tracked)
    {
        var mallocIndex =
            _unityMallocs.FindIndex(x => x.Ptr == ptr && x.Allocator == allocator && x.Tracked == tracked);
        if (mallocIndex < 0) return;

        _unityMallocs.RemoveAt(mallocIndex);
    }

    private readonly struct UnityMallocInfo
    {
        public readonly IntPtr Ptr;
        public readonly Allocator Allocator;
        public readonly bool Tracked;

        public UnityMallocInfo(IntPtr ptr, Allocator allocator, bool tracked)
        {
            Ptr = ptr;
            Allocator = allocator;
            Tracked = tracked;
        }
    }
}