using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;
#if TRACE
using UniTAS.Patcher.ManualServices;
#endif

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SceneManagerAsyncLoadPatch
{
    private const string Namespace = "UnityEngine.SceneManagement";
    private static readonly Type SceneManager = AccessTools.TypeByName($"{Namespace}.SceneManager");
    private static readonly Type UnloadSceneOptions = AccessTools.TypeByName($"{Namespace}.UnloadSceneOptions");
    private static readonly Type LoadSceneParametersType = AccessTools.TypeByName($"{Namespace}.LoadSceneParameters");
    private static readonly Type SceneType = AccessTools.TypeByName($"{Namespace}.Scene");

    private static readonly Type SceneManagerAPIInternal =
        AccessTools.TypeByName($"{Namespace}.SceneManagerAPIInternal");

    private static readonly MethodInfo UnloadSceneNameIndexInternal = UnloadSceneOptions == null
        ? null
        : AccessTools.Method(SceneManagerAPIInternal ?? SceneManager,
              "UnloadSceneNameIndexInternal",
              [typeof(string), typeof(int), typeof(bool), UnloadSceneOptions, typeof(bool).MakeByRefType()]) ??
          AccessTools.Method(SceneManagerAPIInternal ?? SceneManager,
              "UnloadSceneNameIndexInternal",
              [typeof(string), typeof(int), typeof(bool), typeof(bool).MakeByRefType()]);

    private static readonly MethodInfo LoadSceneAsyncNameIndexInternalInjected =
        SceneManagerAPIInternal == null || LoadSceneParametersType == null
            ? null
            : AccessTools.Method(
                SceneManagerAPIInternal, "LoadSceneAsyncNameIndexInternal_Injected",
                [typeof(string), typeof(int), LoadSceneParametersType.MakeByRefType(), typeof(bool)]);

    private static readonly ISceneLoadTracker SceneLoadTracker =
        ContainerStarter.Kernel.GetInstance<ISceneLoadTracker>();

    private static readonly ISceneWrapper SceneWrapper =
        ContainerStarter.Kernel.GetInstance<ISceneWrapper>();

    private static readonly UnityInstanceWrapFactory UnityInstanceWrapFactory =
        ContainerStarter.Kernel.GetInstance<UnityInstanceWrapFactory>();

    private static readonly ISceneLoadInvoke SceneLoadInvoke = ContainerStarter.Kernel.GetInstance<ISceneLoadInvoke>();

    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

    private static readonly IPatchReverseInvoker ReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

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

            Logger.LogDebug($"async scene load, instance id: {__result.GetHashCode()}");
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
                    (LocalPhysicsMode)localPhysicsModeValue, __result);
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
                LocalPhysicsMode.None, __result);
        }

        return false;
    }

    [HarmonyPatch]
    private class UnloadSceneNameIndexInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            return UnloadSceneNameIndexInternal;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(ref bool immediately, out bool __state)
        {
            __state = immediately;
            immediately = true;
        }

        private static void Postfix(ref AsyncOperation __result, bool __state)
        {
            if (__state) return;

            __result = new();
            SceneLoadTracker.AsyncSceneUnload(__result);
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternalInjected
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManager, "UnloadSceneAsyncInternal_Injected", [SceneType.MakeByRefType()]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static readonly MethodInfo _getName = SceneType.GetProperty("name", AccessTools.all)?.GetGetMethod();

        private static bool Prefix(object scene, ref AsyncOperation __result)
        {
            var sceneName = (string)_getName.Invoke(scene, null);

            StaticLogger.LogDebug($"async scene unload, forcing scene `{sceneName}` to unload");
            StaticLogger.LogWarning(
                "THIS OPERATION MIGHT BREAK THE GAME, scene unloading patch is using an unstable unity function, and it may fail");
            var args = new object[] { sceneName, -1, true, null };
            UnloadSceneNameIndexInternal.Invoke(null, args);
            if (!(bool)args[4])
                StaticLogger.LogError("async unload most likely failed, prepare for game to go nuts");

            __result = new();
            return false;
        }

        private static void Postfix(ref AsyncOperation __result)
        {
            __result = new();
            SceneLoadTracker.AsyncSceneUnload(__result);
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternalInjected_NoOptions
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManager, "UnloadSceneAsyncInternal_Injected",
                [SceneType.MakeByRefType(), UnloadSceneOptions]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static readonly MethodInfo _getName =
            SceneType?.GetProperty("name", AccessTools.all)?.GetGetMethod();

        private static bool Prefix(object scene, object options, ref AsyncOperation __result)
        {
            var sceneName = (string)_getName.Invoke(scene, null);

            StaticLogger.LogDebug($"async scene unload, forcing scene `{sceneName}` to unload");
            StaticLogger.LogWarning(
                "THIS OPERATION MIGHT BREAK THE GAME, scene unloading patch is using an unstable unity function, and it may fail");
            var args = new[] { sceneName, -1, true, options, null };
            UnloadSceneNameIndexInternal.Invoke(null, args);
            if (!(bool)args[4])
                StaticLogger.LogError("async unload most likely failed, prepare for game to go nuts");

            __result = new();
            return false;
        }

        private static void Postfix(ref AsyncOperation __result)
        {
            __result = new();
            SceneLoadTracker.AsyncSceneUnload(__result);
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
            __result = SceneWrapper.SceneCount;
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
            // check if it came from the specific method, since we want real data for this
            var frames = new StackTrace().GetFrames();
            if (frames != null)
            {
                foreach (var frame in frames)
                {
                    var method = frame.GetMethod();
                    if (method.DeclaringType == SceneManager && method.Name == "LoadScene")
                        return true;
                }
            }

            __result = SceneWrapper.SceneCount + SceneLoadTracker.LoadingSceneCount;
            return false;
        }
    }
}