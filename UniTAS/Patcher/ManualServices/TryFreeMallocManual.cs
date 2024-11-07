using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices;

public static class TryFreeMallocManual
{
    private static readonly List<UnityMallocInfo> UnityMallocs = [];

    private static readonly MethodBase UnityFree =
        AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:Free");

    private static readonly MethodBase UnityFreeTracked =
        AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:FreeTracked");

    private static readonly Type Allocator = AccessTools.TypeByName("Unity.Collections.Allocator");

    private static readonly bool HasUnityMalloc;

    static TryFreeMallocManual()
    {
        HasUnityMalloc = UnityFree != null && UnityFreeTracked != null && Allocator != null;
    }

    private static void TryFree(IntPtr ptr)
    {
        if (HasUnityMalloc && TryFreeUnityMalloc(ptr)) return;

        StaticLogger.LogWarning($"Could not find malloc at {ptr} to free");
    }

    public static void TryFree(object instance, FieldInfo fieldInfo)
    {
        if (!fieldInfo.FieldType.IsPointer) return;

        var prevValue = fieldInfo.GetValue(instance);
        unsafe
        {
            var prevValueRaw = (IntPtr)Pointer.Unbox(prevValue);
            TryFree(prevValueRaw);
        }
    }

    private static bool TryFreeUnityMalloc(IntPtr ptr)
    {
        var unityMallocIndex = UnityMallocs.FindIndex(x => x.Ptr == ptr);
        if (unityMallocIndex < 0) return false;

        var unityMalloc = UnityMallocs[unityMallocIndex];

        var allocator = Enum.Parse(Allocator, unityMalloc.MallocAllocator.ToString());
        StaticLogger.LogDebug($"Freeing unity malloc at {ptr} with allocator {allocator}");

        // free
        if (unityMalloc.Tracked)
        {
            UnityFreeTracked.Invoke(null, [ptr, allocator]);
        }
        else
        {
            UnityFree.Invoke(null, [ptr, allocator]);
        }

        return true;
    }

    public static void Malloc(IntPtr ptr, Allocator allocator, bool tracked)
    {
        UnityMallocs.Add(new(ptr, allocator, tracked));
    }

    public static void Free(IntPtr ptr, Allocator allocator, bool tracked)
    {
        var mallocIndex =
            UnityMallocs.FindIndex(x => x.Ptr == ptr && x.MallocAllocator == allocator && x.Tracked == tracked);
        if (mallocIndex < 0) return;

        UnityMallocs.RemoveAt(mallocIndex);
    }

    private readonly struct UnityMallocInfo(IntPtr ptr, Allocator mallocAllocator, bool tracked)
    {
        public readonly IntPtr Ptr = ptr;
        public readonly Allocator MallocAllocator = mallocAllocator;
        public readonly bool Tracked = tracked;
    }
}