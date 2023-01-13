using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.AsyncSceneLoadTracker;
using UniTASPlugin.Patches.PatchTypes;
using UnityEngine;

namespace UniTASPlugin.Patches.RawPatches;

[RawPatch]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SceneManagerAsyncLoadPatch
{
    private const string Namespace = "UnityEngine.SceneManagement";
    private static readonly Type SceneManager = AccessTools.TypeByName($"{Namespace}.SceneManager");
    private static readonly Type UnloadSceneOptions = AccessTools.TypeByName($"{Namespace}.UnloadSceneOptions");

    private static readonly MethodInfo UnloadSceneNameIndexInternal = AccessTools.Method(SceneManager,
        "UnloadSceneNameIndexInternal",
        new[] { typeof(string), typeof(int), typeof(bool), UnloadSceneOptions, typeof(bool) });

    private static readonly ISceneLoadTracker SceneLoadTracker = Plugin.Kernel.GetInstance<ISceneLoadTracker>();

    private static bool AsyncSceneLoad(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex,
        object parameters, bool? isAdditive, ref AsyncOperation __result)
    {
        if (mustCompleteNextFrame) return true;
        Trace.Write($"async scene load, instance id: {__result.GetHashCode()}");
        SceneLoadTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, parameters, isAdditive, __result);
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
            UnloadSceneNameIndexInternal.Invoke(null, new[] { "", sceneBuildIndex, true, options, null });
            return false;
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            // string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame
            return AccessTools.Method(SceneManager, "LoadSceneAsyncNameIndexInternal",
                new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame,
            ref AsyncOperation __result)
        {
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, null, isAdditive,
                ref __result);
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternalPatch2
    {
        private static MethodBase TargetMethod()
        {
            // string sceneName, int sceneBuildIndex, LoadSceneParameters parameters, bool mustCompleteNextFrame
            var loadSceneParametersType = AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");
            return AccessTools.Method(SceneManager, "LoadSceneAsyncNameIndexInternal",
                new[] { typeof(string), typeof(int), loadSceneParametersType, typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex, object parameters,
            ref AsyncOperation __result)
        {
            return AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, parameters, null,
                ref __result);
        }
    }
}