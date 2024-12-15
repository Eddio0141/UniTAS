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

        private static bool Prefix(ref bool __result, int ___m_Handle)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => true, actual => actual.IsValid);
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

        private static bool Prefix(ref string __result, int ___m_Handle)
        {
            if (PatchReverseInvoker.Invoking) return true;

            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.TrackingHandle != ___m_Handle) continue;
                CheckAndWarnAPIUsage();
                __result = loading.LoadingScene.Name;
                return false;
            }

            foreach (var loading in SceneLoadTracker.DummyScenes)
            {
                if (loading.dummyScene.TrackingHandle != ___m_Handle) continue;
                __result = loading.actualScene.Name;
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

        private static bool Prefix(int ___m_Handle)
        {
            if (PatchReverseInvoker.Invoking) return true;

            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.TrackingHandle != ___m_Handle) continue;
                CheckAndWarnAPIUsage();
                // well this ain't the proper error, but it should do the same thing
                throw new InvalidOperationException(
                    $"Setting a name on a saved scene is not allowed (the filename is used as name). Scene: '{loading.LoadingScene.Path}'");
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

        private static bool Prefix(int ___m_Handle, ref bool __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => false, actual => actual.IsLoaded);
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

        private static bool Prefix(int ___m_Handle, ref int __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, dummy => dummy.BuildIndex,
                actual => actual.BuildIndex);
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

        private static bool Prefix(int ___m_Handle, ref bool __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => false, actual => actual.IsDirty);
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

        private static bool Prefix(int ___m_Handle, ref int __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => 0, actual => actual.RootCount);
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

        private static bool Prefix(int ___m_Handle, ref bool __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => false, actual => actual.IsSubScene);
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

        private static bool Prefix(int ___m_Handle, ref string __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, dummy => dummy.Path, actual => actual.Path);
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

        private static bool Prefix(int ___m_Handle, ref int __result)
        {
            return CheckAndSetDefault(___m_Handle, ref __result, _ => 0, actual => actual.Handle);
        }
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
            __result = actualSet(loading.actualScene);
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