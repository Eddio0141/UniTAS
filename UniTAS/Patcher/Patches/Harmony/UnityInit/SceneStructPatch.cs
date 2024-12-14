using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SceneStructPatch
{
    private static readonly Type SceneType = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    private static readonly ISceneLoadTracker SceneLoadTracker =
        ContainerStarter.Kernel.GetInstance<ISceneLoadTracker>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly IUnityInstanceWrapFactory WrapFactory =
        ContainerStarter.Kernel.GetInstance<IUnityInstanceWrapFactory>();

    [HarmonyPatch]
    private class IsValid
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "IsValid", []);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_name
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "name");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref string __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = loadInfo.loadingScene.Name;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class set_name
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(SceneType, "name");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;

                CheckAndWarnAPIUsage();
                // well this ain't the proper error, but it should do the same thing
                throw new InvalidOperationException(
                    $"Setting a name on a saved scene is not allowed (the filename is used as name). Scene: '{loadInfo.loadingScene.Path}'");
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_isLoaded
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "isLoaded");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;

                CheckAndWarnAPIUsage();
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_buildIndex
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "buildIndex");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref int __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = loadInfo.loadingScene.BuildIndex;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_isDirty
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "isDirty");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_rootCount
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "rootCount");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref int __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = 0;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_isSubScene
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "isSubScene");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_path
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "path");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref string __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceAddr = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceAddr) continue;
                CheckAndWarnAPIUsage();
                __result = loadInfo.loadingScene.Path;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class op_Equality
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "op_Equality");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object lhs, ref object rhs, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var lhsAddr = WrapFactory.Create<SceneWrapper>(lhs).Handle;
            var rhsAddr = WrapFactory.Create<SceneWrapper>(rhs).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != lhsAddr && loadInfo.trackingHandle != rhsAddr) continue;
                __result = lhsAddr == rhsAddr;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class op_Inequality
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "op_Inequality");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object lhs, ref object rhs, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var lhsAddr = WrapFactory.Create<SceneWrapper>(lhs).Handle;
            var rhsAddr = WrapFactory.Create<SceneWrapper>(rhs).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != lhsAddr && loadInfo.trackingHandle != rhsAddr) continue;
                __result = lhsAddr != rhsAddr;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class GetHashCodePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "GetHashCode");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, ref int __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var sceneWrapped = WrapFactory.Create<SceneWrapper>(__instance);
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != sceneWrapped.Handle) continue;
                __result = 0;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class EqualsPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "Equals");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance, object other, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceId = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            var otherId = WrapFactory.Create<SceneWrapper>(other).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceId && loadInfo.trackingHandle != otherId) continue;
                __result = instanceId == otherId;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class get_handle
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(SceneType, "handle");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __instance)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var instanceId = WrapFactory.Create<SceneWrapper>(__instance).Handle;
            foreach (var loadInfo in SceneLoadTracker.LoadingScenes)
            {
                if (loadInfo.trackingHandle != instanceId) continue;
                CheckAndWarnAPIUsage();
                return false;
            }

            return true;
        }
    }

    private static void CheckAndWarnAPIUsage()
    {
        var frames = new StackTrace(true).GetFrames();
        if (frames == null) return;

        foreach (var frame in frames.Skip(1))
        {
            var method = frame.GetMethod();
            if (method.DeclaringType?.Namespace == typeof(HarmonyLib.Harmony).Namespace) continue;

            if (method.DeclaringType != SceneType)
            {
                StaticLogger.LogWarning(
                    $"Something is calling scene struct API that is managed by UniTAS outside of Scene struct, this could be bad, stacktrace: {new StackTrace()}");
            }

            return;
        }
    }
}