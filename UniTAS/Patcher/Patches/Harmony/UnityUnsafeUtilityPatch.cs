using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class UnityUnsafeUtilityPatch
{
    private static readonly IUnityMallocTracker UnityMallocTracker =
        ContainerStarter.Kernel.GetInstance<IUnityMallocTracker>();

    [HarmonyPatch]
    private class Malloc
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:Malloc");
        }

        private static unsafe void Postfix(void* __result, object allocator)
        {
            var allocatorTranslated = (Allocator)Enum.Parse(typeof(Allocator), allocator.ToString());
            UnityMallocTracker.Malloc((IntPtr)__result, allocatorTranslated, false);
        }
    }

    [HarmonyPatch]
    private class MallocTracked
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:MallocTracked");
        }

        private static unsafe void Postfix(void* __result, object allocator)
        {
            var allocatorTranslated = (Allocator)Enum.Parse(typeof(Allocator), allocator.ToString());
            UnityMallocTracker.Malloc((IntPtr)__result, allocatorTranslated, true);
        }
    }

    [HarmonyPatch]
    private class Free
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:Free");
        }

        private static unsafe void Prefix(void* memory, object allocator)
        {
            var allocatorTranslated = (Allocator)Enum.Parse(typeof(Allocator), allocator.ToString());
            UnityMallocTracker.Free((IntPtr)memory, allocatorTranslated, false);
        }
    }

    [HarmonyPatch]
    private class FreeTracked
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("Unity.Collections.LowLevel.Unsafe.UnsafeUtility:FreeTracked");
        }

        private static unsafe void Prefix(void* memory, object allocator)
        {
            var allocatorTranslated = (Allocator)Enum.Parse(typeof(Allocator), allocator.ToString());
            UnityMallocTracker.Free((IntPtr)memory, allocatorTranslated, true);
        }
    }
}