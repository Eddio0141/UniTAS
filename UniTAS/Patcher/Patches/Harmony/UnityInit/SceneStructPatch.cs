using System;
using System.Collections.Generic;
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

    private static readonly ISceneOverride SceneOverride = ContainerStarter.Kernel.GetInstance<ISceneOverride>();

    [HarmonyPatch]
    private class IsValidInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "IsValidInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result, int sceneHandle)
        {
            return CheckAndSetDefault(sceneHandle, ref __result, _ => true, actual => actual.IsValid);
        }
    }

    [HarmonyPatch]
    private class GetNameInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "GetNameInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref string __result, int sceneHandle)
        {
            return CheckAndSetDefault(sceneHandle, ref __result, x => x.Name, x => x.Name);
        }
    }

    [HarmonyPatch]
    private class SetNameInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "SetNameInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int sceneHandle)
        {
            if (PatchReverseInvoker.Invoking) return true;

            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.TrackingHandle != sceneHandle) continue;
                CheckAndWarnAPIUsage();
                // well this ain't the proper error, but it should do the same thing
                throw new InvalidOperationException(
                    $"Setting a name on a saved scene is not allowed (the filename is used as name). Scene: '{loading.LoadingScene.Path}'");
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class GetBuildIndexInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "GetBuildIndexInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int sceneHandle, ref int __result)
        {
            return CheckAndSetDefault(sceneHandle, ref __result, dummy => dummy.BuildIndex,
                actual => actual.BuildIndex);
        }
    }

    [HarmonyPatch]
    private class GetPathInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "GetPathInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int sceneHandle, ref string __result)
        {
            return CheckAndSetDefault(sceneHandle, ref __result, dummy => dummy.Path, actual => actual.Path);
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

            var rhsAddr = WrapFactory.Create<SceneWrapper>(rhs).Handle;
            var lhsAddr = WrapFactory.Create<SceneWrapper>(lhs).Handle;
            if (rhsAddr == lhsAddr)
            {
                __result = true;
                return false;
            }

            foreach (var loading in SceneLoadTracker.DummyScenes)
            {
                if (loading.dummyScene.TrackingHandle == lhsAddr)
                {
                    __result = loading.actualScene != null && loading.actualScene.Handle == rhsAddr;
                    return false;
                }

                if (loading.dummyScene.TrackingHandle == rhsAddr)
                {
                    __result = loading.actualScene != null && loading.actualScene.Handle == lhsAddr;
                    return false;
                }
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

            var rhsAddr = WrapFactory.Create<SceneWrapper>(rhs).Handle;
            var lhsAddr = WrapFactory.Create<SceneWrapper>(lhs).Handle;
            if (rhsAddr == lhsAddr)
            {
                __result = false;
                return false;
            }

            foreach (var loading in SceneLoadTracker.DummyScenes)
            {
                if (loading.dummyScene.TrackingHandle == lhsAddr)
                {
                    __result = loading.actualScene != null && loading.actualScene.Handle != rhsAddr;
                    return false;
                }

                if (loading.dummyScene.TrackingHandle == rhsAddr)
                {
                    __result = loading.actualScene != null && loading.actualScene.Handle != lhsAddr;
                    return false;
                }
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

        private static bool Prefix(int ___m_Handle, ref int __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => 0, actual => actual.Instance.GetHashCode());
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

        private static bool Prefix(int ___m_Handle, object other, ref bool __result)
        {
            if (PatchReverseInvoker.Invoking) return true;

            var otherHandle = WrapFactory.Create<SceneWrapper>(other).Handle;
            if (___m_Handle == otherHandle)
            {
                __result = true;
                return false;
            }

            foreach (var loading in SceneLoadTracker.DummyScenes)
            {
                if (loading.dummyScene.TrackingHandle == ___m_Handle)
                {
                    __result = loading.actualScene != null && loading.actualScene.Handle == otherHandle;
                    return false;
                }

                if (loading.dummyScene.TrackingHandle == otherHandle)
                {
                    __result = loading.actualScene != null && loading.actualScene.Handle == ___m_Handle;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class GetIsLoadedInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneType, "GetIsLoadedInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int sceneHandle, ref bool __result)
        {
            if (SceneOverride.IsLoaded(sceneHandle, out var loaded))
            {
                __result = loaded;
                return false;
            }

            return CheckAndSetDefault(sceneHandle, ref __result, _ => false, actual => actual.IsLoaded);
        }
    }

    [HarmonyPatch]
    private class FixHandleOrReturnDefault
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            return new[]
            {
                AccessTools.Method(SceneType, "GetRootGameObjectsInternal"),
                AccessTools.Method(SceneType, "GetRootCountInternal"),
                AccessTools.Method(SceneType, "GetDirtyID"),
                AccessTools.Method(SceneType, "GetIsDirtyInternal"),

                // these shouldn't matter
                AccessTools.Method(SceneType, "GetLoadingStateInternal"),
                AccessTools.Method(SceneType, "GetGUIDInternal"),
                AccessTools.Method(SceneType, "SetPathAndGUIDInternal"),

                AccessTools.Method(SceneType, "IsSubScene")
                // TODO: SetIsSubScene, IsSubScene
            }.Where(x => x != null);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int sceneHandle)
        {
            return CheckAndSetHandleArg(ref sceneHandle);
        }
    }

    private static bool CheckAndSetHandleArg(ref int sceneHandle)
    {
        foreach (var dummy in SceneLoadTracker.DummyScenes)
        {
            if (dummy.dummyScene.TrackingHandle != sceneHandle) continue;
            CheckAndWarnAPIUsage();
            if (dummy.actualScene == null)
                return false;

            sceneHandle = dummy.actualScene.Handle;
            return true;
        }

        return true;
    }

    private static bool CheckAndSetHandleArg<T>(ref int sceneHandle, ref T __result, Func<T> dummySet)
    {
        foreach (var dummy in SceneLoadTracker.DummyScenes)
        {
            if (dummy.dummyScene.TrackingHandle != sceneHandle) continue;
            CheckAndWarnAPIUsage();
            if (dummy.actualScene == null)
            {
                __result = dummySet();
                return false;
            }

            sceneHandle = dummy.actualScene.Handle;
            return true;
        }

        return true;
    }

    private static bool CheckAndSetDefault<T>(int mHandle, ref T __result,
        Func<AsyncOperationTracker.SceneInfo, T> dummySet, Func<SceneWrapper, T> actualSet)
    {
        if (PatchReverseInvoker.Invoking) return true;

        foreach (var loading in SceneLoadTracker.LoadingScenes)
        {
            if (loading.TrackingHandle != mHandle) continue;
            CheckAndWarnAPIUsage();
            __result = dummySet(loading.LoadingScene);
            return false;
        }

        foreach (var loading in SceneLoadTracker.DummyScenes)
        {
            if (loading.dummyScene.TrackingHandle != mHandle) continue;
            __result = PatchReverseInvoker.Invoke((a, b) => a(b), actualSet, loading.actualScene);
            return false;
        }

        return true;
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