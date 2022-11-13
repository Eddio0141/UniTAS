using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.VersionSafeWrapper;
using AsyncOpOrig = UnityEngine.AsyncOperation;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace UniTASPlugin.Patches.UnityEngine.SceneManagment;

[HarmonyPatch]
internal static class SceneManager
{
    internal static class Helper
    {
        private const string Namespace = "UnityEngine.SceneManagement";

        public static Type GetSceneManager()
        {
            return AccessTools.TypeByName($"{Namespace}.SceneManager");
        }

        public static Type GetUnloadSceneOptions()
        {
            return AccessTools.TypeByName($"{Namespace}.UnloadSceneOptions");
        }

        public static MethodInfo GetUnloadSceneNameIndexInternal()
        {
            return AccessTools.Method(GetSceneManager(), "UnloadSceneNameIndexInternal", new[] { typeof(string), typeof(int), typeof(bool), GetUnloadSceneOptions(), typeof(bool) });
        }

        public static bool AsyncSceneLoad(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive, ref AsyncOpOrig __result)
        {
            if (mustCompleteNextFrame) return true;
            Plugin.Instance.Log.LogDebug("async scene load");
            __result = new AsyncOpOrig();
            var wrap = new AsyncOperationWrap(__result);
            wrap.AssignUID();
            GameTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, parameters, isAdditive, wrap);
            Plugin.Instance.Log.LogDebug($"setting up async scene load, assigned UID {wrap.UID}");
            return false;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneNameIndexInternal
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneNameIndexInternal");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Prefix(ref bool immediately)
        {
            immediately = true;
        }
    }

    [HarmonyPatch]
    private class UnloadSceneAsyncInternal_Injected
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsyncInternal_Injected");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(object scene, object options)
        {
            var sceneTraverse = Traverse.Create(scene);
            var sceneBuildIndex = sceneTraverse.Property("buildIndex").GetValue<int>();
            _ = Helper.GetUnloadSceneNameIndexInternal().Invoke(null, new[] { "", sceneBuildIndex, true, options, null });
            return false;
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternal__sceneName__sceneBuildIndex__isAdditive__mustCompleteNextFrame
    {
        private static MethodBase TargetMethod()
        {
            // string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame
            return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsyncNameIndexInternal", new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame, ref AsyncOpOrig __result)
        {
            return Helper.AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, null, isAdditive, ref __result);
        }
    }

    [HarmonyPatch]
    private class LoadSceneAsyncNameIndexInternal__sceneName__sceneBuildIndex__parameters__mustCompleteNextFrame
    {
        private static MethodBase TargetMethod()
        {
            // string sceneName, int sceneBuildIndex, LoadSceneParameters parameters, bool mustCompleteNextFrame
            var loadSceneParametersType = AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");
            return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsyncNameIndexInternal", new[] { typeof(string), typeof(int), loadSceneParametersType, typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex, object parameters, ref AsyncOpOrig __result)
        {
            return Helper.AsyncSceneLoad(mustCompleteNextFrame, sceneName, sceneBuildIndex, parameters, null, ref __result);
        }
    }
}
