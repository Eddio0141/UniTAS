using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.Trackers.SceneIndexNameTracker;
using UniTASPlugin.Trackers.SceneTracker;
using UniTASPlugin.UnitySafeWrappers;
using UniTASPlugin.UnitySafeWrappers.Wrappers;
using UniTASPlugin.UnitySafeWrappers.Wrappers.SceneManagement;
using UnityEngine;

namespace UniTASPlugin.Patches.RawPatches;

// TODO disable this patch for now
// [RawPatch(1000)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SceneTrackerPatch
{
    private static readonly ISceneTracker SceneTracker = Plugin.Kernel.GetInstance<ISceneTracker>();
    private static readonly ISceneIndexName SceneIndexName = Plugin.Kernel.GetInstance<ISceneIndexName>();

    private static readonly IUnityInstanceWrapFactory UnityInstanceWrapFactory =
        Plugin.Kernel.GetInstance<IUnityInstanceWrapFactory>();

    private const string Namespace = "UnityEngine.SceneManagement";

    private static readonly Type SceneManagerInternalType =
        AccessTools.TypeByName($"{Namespace}.SceneManagerAPIInternal");

    private static readonly Type SceneManagerType = AccessTools.TypeByName($"{Namespace}.SceneManager");

    private static readonly Type LoadSceneParametersType = AccessTools.TypeByName($"{Namespace}.LoadSceneParameters");

    private static readonly Type SceneType = AccessTools.TypeByName($"{Namespace}.Scene");

    // list of additive scene arg names
    // Application.LoadLevelAsync -> bool additive
    // SceneManagerAPIInternal -> LoadSceneParameters loadSceneParameters -> loadSceneMode == LoadSceneMode.Additive
    // SceneManager.LoadSceneAsync -> bool isAdditive

    [HarmonyPatch]
    private class SceneManagerAPIInternalLoadPatch
    {
        private static MethodBase TargetMethod()
        {
            return SceneManagerInternalType.GetMethod("LoadSceneAsyncNameIndexInternal_Injected",
                AccessTools.all,
                null,
                new[] { typeof(string), typeof(int), LoadSceneParametersType.MakeByRefType(), typeof(bool) }, null);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(string sceneName, int sceneBuildIndex, object parameters)
        {
            var instance = UnityInstanceWrapFactory.Create<LoadSceneParametersWrapper>(parameters);
            var additive = instance.LoadSceneMode == LoadSceneMode.Additive;
            var sceneIndex = sceneBuildIndex != -1
                ? sceneBuildIndex
                : SceneIndexName.GetSceneIndex(sceneName) ??
                  throw new InvalidOperationException("Scene index not found");
            var name = sceneName ?? SceneIndexName.GetSceneName(sceneIndex) ??
                throw new InvalidOperationException("Scene name not found");
            SceneTracker.LoadScene(sceneIndex, name, additive);
        }
    }

    [HarmonyPatch]
    private class ApplicationLoadLevelAsyncPatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(Application).GetMethod(nameof(Application.LoadLevelAsync),
                new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(string monoLevelName, int index, bool additive)
        {
            // additive in older versions of unity simply adds the object to the current scene
            if (additive) return;
            var sceneIndex = index != -1
                ? index
                : SceneIndexName.GetSceneIndex(monoLevelName) ??
                  throw new InvalidOperationException("Scene index not found");
            var name = monoLevelName ?? SceneIndexName.GetSceneName(sceneIndex) ??
                throw new InvalidOperationException("Scene name not found");
            SceneTracker.LoadScene(sceneIndex, name, false);
        }
    }

    private const string loadSceneAsyncNameIndexInternal = "LoadSceneAsyncNameIndexInternal";

    [HarmonyPatch]
    private class SceneManagerLoadSceneAsyncPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManagerType, loadSceneAsyncNameIndexInternal,
                new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(string sceneName, int sceneBuildIndex, bool isAdditive)
        {
            var sceneIndex = sceneBuildIndex != -1
                ? sceneBuildIndex
                : SceneIndexName.GetSceneIndex(sceneName) ??
                  throw new InvalidOperationException("Scene index not found");
            var name = sceneName ?? SceneIndexName.GetSceneName(sceneIndex) ??
                throw new InvalidOperationException("Scene name not found");
            SceneTracker.LoadScene(sceneIndex, name, isAdditive);
        }
    }

    [HarmonyPatch]
    private class SceneManagerLoadSceneAsyncPatch2
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(SceneManagerType, loadSceneAsyncNameIndexInternal,
                new[] { typeof(string), typeof(int), LoadSceneParametersType, typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(string sceneName, int sceneBuildIndex, object parameters)
        {
            var sceneIndex = sceneBuildIndex != -1
                ? sceneBuildIndex
                : SceneIndexName.GetSceneIndex(sceneName) ??
                  throw new InvalidOperationException("Scene index not found");
            var name = sceneName ?? SceneIndexName.GetSceneName(sceneIndex) ??
                throw new InvalidOperationException("Scene name not found");
            var instance = UnityInstanceWrapFactory.Create<LoadSceneParametersWrapper>(parameters);
            SceneTracker.LoadScene(sceneIndex, name,
                instance.LoadSceneMode == LoadSceneMode.Additive);
        }
    }

    // SceneManager
    // INTERNAL_CALL_UnloadSceneInternal(ref Scene scene)
    // INTERNAL_CALL_UnloadSceneAsyncInternal(ref Scene scene)
    // UnloadSceneNameIndexInternal(string sceneName, int sceneBuildIndex, bool immediately, out bool outSuccess)

    // SceneManagerAPIInternal
    // UnloadSceneNameIndexInternal(string sceneName, int sceneBuildIndex, bool immediately, UnloadSceneOptions options, out bool outSuccess)

    [HarmonyPatch]
    private class SceneManagerUnloadSceneAsyncInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            return SceneManagerType.GetMethod("UnloadSceneNameIndexInternal",
                AccessTools.all,
                null,
                new[] { typeof(string), typeof(int), typeof(bool), typeof(bool).MakeByRefType() }, null);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(string sceneName, int sceneBuildIndex)
        {
            var sceneIndex = sceneBuildIndex != -1
                ? sceneBuildIndex
                : SceneIndexName.GetSceneIndex(sceneName) ??
                  throw new InvalidOperationException("Scene index not found");
            var name = sceneName ?? SceneIndexName.GetSceneName(sceneIndex) ??
                throw new InvalidOperationException("Scene name not found");
            SceneTracker.UnloadScene(sceneIndex, name);
        }
    }

    [HarmonyPatch]
    private class InternalCallSceneManagerUnloadSceneInternalPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return SceneManagerType.GetMethod("INTERNAL_CALL_UnloadSceneInternal",
                AccessTools.all,
                null,
                new[] { SceneType.MakeByRefType() }, null);
            yield return SceneManagerType.GetMethod("INTERNAL_CALL_UnloadSceneAsyncInternal",
                AccessTools.all,
                null,
                new[] { SceneType.MakeByRefType() }, null);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(object scene)
        {
            var sceneWrap = UnityInstanceWrapFactory.Create<SceneWrapper>(scene);
            SceneTracker.UnloadScene(sceneWrap.BuildIndex, sceneWrap.Name);
        }
    }

    [HarmonyPatch]
    private class InternalCallSceneManagerUnloadSceneAsyncInternalPatch
    {
        private static MethodBase TargetMethod()
        {
            var unloadSceneOptionsType = AccessTools.TypeByName($"{Namespace}.UnloadSceneOptions");
            return SceneManagerInternalType.GetMethod("UnloadSceneNameIndexInternal",
                AccessTools.all,
                null,
                new[]
                {
                    typeof(string), typeof(int), typeof(bool), unloadSceneOptionsType, typeof(bool).MakeByRefType()
                }, null);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(string sceneName, int sceneBuildIndex)
        {
            var sceneIndex = sceneBuildIndex != -1
                ? sceneBuildIndex
                : SceneIndexName.GetSceneIndex(sceneName) ??
                  throw new InvalidOperationException("Scene index not found");
            var name = sceneName ?? SceneIndexName.GetSceneName(sceneIndex) ??
                throw new InvalidOperationException("Scene name not found");
            SceneTracker.UnloadScene(sceneIndex, name);
        }
    }
}