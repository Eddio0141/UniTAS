using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Utils;
using UnityEngine;

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

    private static readonly Type SceneManagerAPIInternal =
        AccessTools.TypeByName($"{Namespace}.SceneManagerAPIInternal");

    private static readonly MethodInfo UnloadSceneNameIndexInternal = UnloadSceneOptions == null
        ? null
        : AccessTools.Method(SceneManager,
            "UnloadSceneNameIndexInternal",
            [typeof(string), typeof(int), typeof(bool), UnloadSceneOptions, typeof(bool).MakeByRefType()]);

    private static readonly MethodInfo LoadSceneAsyncNameIndexInternalInjected =
        SceneManagerAPIInternal == null || LoadSceneParametersType == null
            ? null
            : AccessTools.Method(
                SceneManagerAPIInternal, "LoadSceneAsyncNameIndexInternal_Injected",
                [typeof(string), typeof(int), LoadSceneParametersType.MakeByRefType(), typeof(bool)]);

    private static readonly ISceneLoadTracker SceneLoadTracker =
        ContainerStarter.Kernel.GetInstance<ISceneLoadTracker>();

    private static readonly UnityInstanceWrapFactory UnityInstanceWrapFactory =
        ContainerStarter.Kernel.GetInstance<UnityInstanceWrapFactory>();

    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

    private static bool AsyncSceneLoad(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex,
        object parameters, bool? isAdditive, ref AsyncOperation __result)
    {
        if (mustCompleteNextFrame) return true;
        Logger.LogDebug($"async scene load, instance id: {__result.GetHashCode()}");

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

            SceneLoadTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, (LoadSceneMode)loadSceneModeValue,
                (LocalPhysicsMode)localPhysicsModeValue, __result);
            return false;
        }

        if (isAdditive == null)
        {
            throw new ArgumentNullException(nameof(isAdditive));
        }

        SceneLoadTracker.AsyncSceneLoad(sceneName, sceneBuildIndex,
            isAdditive.Value ? LoadSceneMode.Additive : LoadSceneMode.Single,
            LocalPhysicsMode.None, __result);

        return false;
    }

    [HarmonyPatch]
    private class UnloadSceneNameIndexInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManager, "UnloadSceneNameIndexInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(ref bool immediately)
        {
            immediately = true;
        }
    }

    [HarmonyPatch]
    private class APIInternalUnloadSceneNameIndexInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManagerAPIInternal, "UnloadSceneNameIndexInternal",
                [typeof(string), typeof(int), typeof(bool), UnloadSceneOptions, typeof(bool).MakeByRefType()]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(ref bool immediately)
        {
            immediately = true;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternalInjectedPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManager, "UnloadSceneAsyncInternal_Injected");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(object scene, object options)
        {
            var sceneTraverse = Traverse.Create(scene);
            var sceneBuildIndex = sceneTraverse.Property("buildIndex").GetValue<int>();
            UnloadSceneNameIndexInternal.Invoke(null, ["", sceneBuildIndex, true, options, null]);
            return false;
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
            __result = new();
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, null, isAdditive,
                ref __result);
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
            __result = new();
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, parameters, null,
                ref __result);
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
            __result = new();
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, parameters, null,
                ref __result);
        }
    }
}