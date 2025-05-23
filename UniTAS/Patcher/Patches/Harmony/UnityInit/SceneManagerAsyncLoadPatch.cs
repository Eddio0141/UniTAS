using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;
#if TRACE
using Trace = UniTAS.Patcher.ManualServices.Trace;
#endif

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SceneManagerAsyncLoadPatch
{
    static SceneManagerAsyncLoadPatch()
    {
        if (UnloadSceneOptions != null)
        {
            UnloadSceneNameIndexInternal = AccessTools.Method(SceneManagerAPIInternal ?? SceneManager,
                "UnloadSceneNameIndexInternal",
                [typeof(string), typeof(int), typeof(bool), UnloadSceneOptions, typeof(bool).MakeByRefType()]);
            UnloadSceneNameIndexInternalHasOptions = UnloadSceneNameIndexInternal != null;
        }

        UnloadSceneNameIndexInternal ??= AccessTools.Method(SceneManagerAPIInternal ?? SceneManager,
            "UnloadSceneNameIndexInternal",
            [typeof(string), typeof(int), typeof(bool), typeof(bool).MakeByRefType()]);
    }

    private const string Namespace = "UnityEngine.SceneManagement";
    private static readonly Type SceneManager = AccessTools.TypeByName($"{Namespace}.SceneManager");
    private static readonly Type UnloadSceneOptions = AccessTools.TypeByName($"{Namespace}.UnloadSceneOptions");
    private static readonly Type LoadSceneParametersType = AccessTools.TypeByName($"{Namespace}.LoadSceneParameters");
    private static readonly Type SceneType = AccessTools.TypeByName($"{Namespace}.Scene");

    private static readonly Type SceneManagerAPIInternal =
        AccessTools.TypeByName($"{Namespace}.SceneManagerAPIInternal");

    private static readonly MethodInfo UnloadSceneNameIndexInternal;
    private static readonly bool UnloadSceneNameIndexInternalHasOptions;

    private static readonly MethodInfo LoadSceneAsyncNameIndexInternalInjected =
        SceneManagerAPIInternal == null || LoadSceneParametersType == null
            ? null
            : AccessTools.Method(
                SceneManagerAPIInternal, "LoadSceneAsyncNameIndexInternal_Injected",
                [typeof(string), typeof(int), LoadSceneParametersType.MakeByRefType(), typeof(bool)]);

    private static readonly MethodInfo GetSceneByBuildIndex =
        SceneManager?.GetMethod("GetSceneByBuildIndex", AccessTools.all, null, [typeof(int)], null);

    private static readonly MethodInfo SceneGetName = SceneType?.GetProperty("name", AccessTools.all)?.GetGetMethod();

    private static readonly MethodInfo SetActiveSceneInjected = SceneType == null
        ? null
        : SceneManager?.GetMethod("SetActiveScene_Injected", AccessTools.all,
            null, [SceneType.MakeByRefType()], null);

    private static readonly MethodInfo GetSceneByNameInjected = SceneType == null
        ? null
        : SceneManager?.GetMethod("GetSceneByName_Injected", AccessTools.all, null,
            [typeof(string), SceneType.MakeByRefType()], null);

    private static readonly MethodInfo GetSceneByPathInjected = SceneType == null
        ? null
        : SceneManager?.GetMethod("GetSceneByPath_Injected", AccessTools.all,
            null,
            [typeof(string), SceneType.MakeByRefType()], null);

    private static readonly MethodInfo GetSceneByBuildIndexInjected = SceneType == null
        ? null
        : (SceneManagerAPIInternal ?? SceneManager)?.GetMethod("GetSceneByBuildIndex_Injected", AccessTools.all,
            null,
            [typeof(int), SceneType.MakeByRefType()], null);

    private static readonly MethodInfo GetSceneAtInjected = SceneType == null
        ? null
        : SceneManager?.GetMethod("GetSceneAt_Injected", AccessTools.all, null,
            [typeof(int), SceneType.MakeByRefType()], null);

    private static readonly ISceneLoadTracker SceneLoadTracker =
        ContainerStarter.Kernel.GetInstance<ISceneLoadTracker>();

    private static readonly ISceneManagerWrapper SceneManagerWrapper =
        ContainerStarter.Kernel.GetInstance<ISceneManagerWrapper>();

    private static readonly UnityInstanceWrapFactory UnityInstanceWrapFactory =
        ContainerStarter.Kernel.GetInstance<UnityInstanceWrapFactory>();

    private static readonly ISceneLoadInvoke SceneLoadInvoke = ContainerStarter.Kernel.GetInstance<ISceneLoadInvoke>();

    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

    private static readonly IPatchReverseInvoker ReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly IUnityInstanceWrapFactory WrapFactory =
        ContainerStarter.Kernel.GetInstance<IUnityInstanceWrapFactory>();

    private static bool AsyncSceneLoad(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex,
        object parameters, bool? isAdditive, ref AsyncOperation __result)
    {
#if TRACE
        using var _ = Trace.MethodStart(methodArgs:
        [
            (nameof(mustCompleteNextFrame), mustCompleteNextFrame), (nameof(sceneName), sceneName),
            (nameof(sceneBuildIndex), sceneBuildIndex),
            (nameof(parameters), parameters), (nameof(isAdditive), isAdditive)
        ]);
#endif

        // everything goes through here, so yeah why not
        if (ReverseInvoker.Invoking)
        {
            SceneLoadInvoke.SceneLoadCall();
            return true;
        }

        // if mustCompleteNextFrame is true, time to do the wacky thing unity does!!!
        //
        // https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html
        /*
         * Because loading is set to complete in the next rendered frame, calling SceneManager.LoadScene
         * forces all previous AsyncOperations to complete, even if AsyncOperation.allowSceneActivation is set to false
         */
        if (!mustCompleteNextFrame)
        {
            __result = new();
        }

        if (parameters != null)
        {
            var instance = UnityInstanceWrapFactory.Create<LoadSceneParametersWrapper>(parameters);

            if (instance.LoadSceneMode == null ||
                instance.LocalPhysicsMode == null)
            {
                throw new InvalidOperationException("Property shouldn't be null here");
            }

            var loadSceneModeValue = (int)instance.LoadSceneMode;
            var localPhysicsModeValue = (int)instance.LocalPhysicsMode;

            if (mustCompleteNextFrame)
            {
                SceneLoadTracker.NonAsyncSceneLoad(sceneName, sceneBuildIndex, (LoadSceneMode)loadSceneModeValue,
                    (LocalPhysicsMode)localPhysicsModeValue);
            }
            else
            {
                SceneLoadTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, (LoadSceneMode)loadSceneModeValue,
                    (LocalPhysicsMode)localPhysicsModeValue, ref __result);
            }

            return false;
        }

        if (isAdditive == null)
        {
            throw new ArgumentNullException(nameof(isAdditive));
        }

        if (mustCompleteNextFrame)
        {
            SceneLoadTracker.NonAsyncSceneLoad(sceneName, sceneBuildIndex,
                isAdditive.Value ? LoadSceneMode.Additive : LoadSceneMode.Single, LocalPhysicsMode.None);
        }
        else
        {
            SceneLoadTracker.AsyncSceneLoad(sceneName, sceneBuildIndex,
                isAdditive.Value ? LoadSceneMode.Additive : LoadSceneMode.Single,
                LocalPhysicsMode.None, ref __result);
        }

        return false;
    }

    [HarmonyPatch]
    private class UnloadSceneNameIndexInternalPatchOptions
    {
        private static bool Prepare() => UnloadSceneNameIndexInternalHasOptions;

        private static MethodBase TargetMethod()
        {
            return UnloadSceneNameIndexInternal;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string sceneName, int sceneBuildIndex, ref bool outSuccess, object options,
            ref AsyncOperation __result)
        {
            if (ReverseInvoker.Invoking) return true;

            __result = new AsyncOperation();
            SceneLoadTracker.AsyncSceneUnload(ref __result, sceneBuildIndex >= 0 ? sceneBuildIndex : sceneName,
                options);
            outSuccess = __result != null;
            return false;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneNameIndexInternalPatch
    {
        private static bool Prepare() => !UnloadSceneNameIndexInternalHasOptions;

        private static MethodBase TargetMethod()
        {
            return UnloadSceneNameIndexInternal;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string sceneName, int sceneBuildIndex, ref bool outSuccess,
            ref AsyncOperation __result)
        {
            if (ReverseInvoker.Invoking) return true;

            __result = new AsyncOperation();
            SceneLoadTracker.AsyncSceneUnload(ref __result, sceneBuildIndex >= 0 ? sceneBuildIndex : sceneName, null);
            outSuccess = __result != null;
            return false;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternalInjected
    {
        private static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(SceneManager, "UnloadSceneAsyncInternal_Injected",
                [SceneType.MakeByRefType()]);
            if (method == null || method.ReturnType == typeof(IntPtr)) return null;
            return method;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(object scene, ref AsyncOperation __result)
        {
            __result = new();
            SceneLoadTracker.AsyncSceneUnload(ref __result, scene, null);
            return false;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternalInjected_NoOptions
    {
        private static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(SceneManager, "UnloadSceneAsyncInternal_Injected",
                [SceneType.MakeByRefType(), UnloadSceneOptions]);
            if (method == null || method.ReturnType == typeof(IntPtr)) return null;
            return method;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(object scene, object options, ref AsyncOperation __result)
        {
            if (ReverseInvoker.Invoking) return true;

            __result = new();
            SceneLoadTracker.AsyncSceneUnload(ref __result, scene, options);
            return false;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternalInjected_IntPtr
    {
        private static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(SceneManager, "UnloadSceneAsyncInternal_Injected",
                [SceneType.MakeByRefType(), UnloadSceneOptions]);
            if (method == null || method.ReturnType != typeof(IntPtr)) return null;
            return method;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(object scene, object options, ref IntPtr __result)
        {
            if (ReverseInvoker.Invoking) return true;

            SceneLoadTracker.AsyncSceneUnload(out __result, scene, options);
            return false;
        }
    }

    [HarmonyPatch]
    private class ConvertToManaged
    {
        private static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("UnityEngine.AsyncOperation.BindingsMarshaller"),
                "ConvertToManaged");

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(IntPtr ptr, ref AsyncOperation __result)
        {
            __result = SceneLoadTracker.ConvertToManaged(ptr);
            return __result != null;
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            if (LoadSceneAsyncNameIndexInternalInjected != null)
            {
                return null;
            }

            // string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame
            return AccessTools.Method(SceneManager, "LoadSceneAsyncNameIndexInternal",
                [typeof(string), typeof(int), typeof(bool), typeof(bool)]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame,
            ref AsyncOperation __result)
        {
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, null, isAdditive, ref __result);
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternalPatch2
    {
        private static MethodBase TargetMethod()
        {
            if (LoadSceneAsyncNameIndexInternalInjected != null)
            {
                return null;
            }

            // string sceneName, int sceneBuildIndex, LoadSceneParameters parameters, bool mustCompleteNextFrame
            return AccessTools.Method(SceneManager, "LoadSceneAsyncNameIndexInternal",
                [typeof(string), typeof(int), LoadSceneParametersType, typeof(bool)]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex, object parameters,
            ref AsyncOperation __result)
        {
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, parameters, null, ref __result);
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternalInjectedPatch
    {
        private static MethodBase TargetMethod()
        {
            return LoadSceneAsyncNameIndexInternalInjected;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string sceneName, int sceneBuildIndex, object parameters, bool mustCompleteNextFrame,
            ref AsyncOperation __result)
        {
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, parameters, null, ref __result);
        }
    }

    [HarmonyPatch]
    private class LoadLevelAsync
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Application), nameof(Application.LoadLevelAsync),
                [typeof(string), typeof(int), typeof(bool), typeof(bool)]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string monoLevelName, int index, bool additive, bool mustCompleteNextFrame,
            ref AsyncOperation __result)
        {
            return AsyncSceneLoad(mustCompleteNextFrame, monoLevelName, index, null, additive, ref __result);
        }
    }

    [HarmonyPatch]
    private class get_loadedSceneCount
    {
        private static readonly MethodInfo GetLoadedSceneCount =
            SceneManager == null ? null : AccessTools.PropertyGetter(SceneManager, "loadedSceneCount");

        private static MethodBase TargetMethod()
        {
            return GetLoadedSceneCount;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            __result = SceneManagerWrapper.LoadedSceneCountDummy;
            return false;
        }
    }

    [HarmonyPatch]
    private class get_sceneCount
    {
        private static readonly MethodInfo GetSceneCount =
            SceneManager == null ? null : AccessTools.PropertyGetter(SceneManager, "sceneCount");

        private static MethodBase TargetMethod()
        {
            return GetSceneCount;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (ReverseInvoker.Invoking) return true;
            __result = SceneManagerWrapper.LoadedSceneCountDummy + SceneLoadTracker.LoadingSceneCount;
            return false;
        }
    }

    [HarmonyPatch]
    private class GetSceneAt_Injected
    {
        private static MethodBase TargetMethod()
        {
            return GetSceneAtInjected;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int index, ref object ret)
        {
            var sceneCount = SceneManagerWrapper.SceneCount;
            if (index < sceneCount) return true;

            // check loading ones
            var loadIndex = index - sceneCount;
            if (loadIndex >= SceneLoadTracker.LoadingScenes.Count) return true;

            ret = SceneLoadTracker.LoadingScenes[loadIndex].DummySceneStruct;
            return false;
        }
    }

    [HarmonyPatch]
    private class GetSceneByName_Injected
    {
        private static MethodBase TargetMethod()
        {
            return GetSceneByNameInjected;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string name, ref object ret)
        {
            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.LoadingScene.Name != name) continue;
                ret = loading.DummySceneStruct;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class GetSceneByBuildIndex_Injected
    {
        private static MethodBase TargetMethod()
        {
            return GetSceneByBuildIndexInjected;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int buildIndex, ref object ret)
        {
            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.LoadingScene.BuildIndex != buildIndex) continue;
                ret = loading.DummySceneStruct;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class GetSceneByPath_Injected
    {
        private static MethodBase TargetMethod()
        {
            return GetSceneByPathInjected;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string scenePath, ref object ret)
        {
            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.LoadingScene.Path != scenePath) continue;
                ret = loading.DummySceneStruct;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    private class SetActiveScene_Injected
    {
        private static MethodBase TargetMethod()
        {
            return SetActiveSceneInjected;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(ref object scene)
        {
            StaticLogger.Trace($"prefix invoke: {new StackTrace()}");
            var handle = WrapFactory.Create<SceneWrapper>(scene).Handle;
            foreach (var loading in SceneLoadTracker.LoadingScenes)
            {
                if (loading.TrackingHandle != handle) continue;
                // scene is still loading, so do the intended error
                throw new ArgumentException(
                    $"SceneManager.SetActiveScene failed; scene '{loading.LoadingScene.Name}' is not loaded and therefore cannot be set active");
            }

            foreach (var dummy in SceneLoadTracker.DummyScenes)
            {
                if (dummy.dummyScene.TrackingHandle != handle) continue;
                scene = dummy.actualScene.Instance;
                return;
            }
        }
    }

    [HarmonyPatch]
    private class ReversePatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return new[]
            {
                AccessTools.Method(SceneManager, "Internal_SceneLoaded")
            }.Where(x => x != null).Select(MethodBase (x) => x);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix()
        {
            ReverseInvoker.Invoking = true;
        }

        private static void Postfix()
        {
            ReverseInvoker.Invoking = false;
        }
    }
}