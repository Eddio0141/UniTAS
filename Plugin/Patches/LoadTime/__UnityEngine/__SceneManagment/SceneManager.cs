using HarmonyLib;
using System;
using UnityEngine;
//using UnityEngine.SceneManagement;

namespace UniTASPlugin.Patches.LoadTime.__UnityEngine.__SceneManagment;

/*
[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadSceneAsync), new Type[] { typeof(int), typeof(LoadSceneParameters) })]
class LoadSceneAsync__sceneBuildIndex__parameters
{
    static bool Prefix(int sceneBuildIndex, LoadSceneParameters parameters, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.LoadSceneAsyncNameIndexInternal(null, sceneBuildIndex, parameters, true);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneLoad(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadSceneAsync), new Type[] { typeof(string), typeof(LoadSceneParameters) })]
class LoadSceneAsync__sceneName__parameters
{
    static bool Prefix(string sceneName, LoadSceneParameters parameters, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.LoadSceneAsyncNameIndexInternal(sceneName, -1, parameters, true);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneLoad(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.UnloadSceneAsync), new Type[] { typeof(int) })]
class UnloadSceneAsync__sceneBuildIndex
{
    static bool Prefix(int sceneBuildIndex, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.UnloadSceneNameIndexInternal("", sceneBuildIndex, true, UnloadSceneOptions.None, out _);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.UnloadSceneAsync), new Type[] { typeof(string) })]
class UnloadSceneAsync__sceneName
{
    static bool Prefix(string sceneName, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.UnloadSceneNameIndexInternal(sceneName, -1, true, UnloadSceneOptions.None, out _);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.UnloadSceneAsync), new Type[] { typeof(Scene) })]
class UnloadSceneAsync__scene
{
    static bool Prefix(ref Scene scene, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.UnloadSceneNameIndexInternal("", scene.buildIndex, true, UnloadSceneOptions.None, out _);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.UnloadSceneAsync), new Type[] { typeof(int), typeof(UnloadSceneOptions) })]
class UnloadSceneAsync__sceneBuildIndex__options
{
    static bool Prefix(int sceneBuildIndex, UnloadSceneOptions options, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.UnloadSceneNameIndexInternal("", sceneBuildIndex, true, options, out _);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.UnloadSceneAsync), new Type[] { typeof(string), typeof(UnloadSceneOptions) })]
class UnloadSceneAsync__sceneName__options
{
    static bool Prefix(string sceneName, UnloadSceneOptions options, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.UnloadSceneNameIndexInternal(sceneName, -1, true, options, out _);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.UnloadSceneAsync), new Type[] { typeof(Scene), typeof(UnloadSceneOptions) })]
class UnloadSceneAsync__scene__options
{
    static bool Prefix(ref Scene scene, UnloadSceneOptions options, ref AsyncOperation __result)
    {
        if (UniTASPlugin.TAS.Main.Running)
        {
            __result = SceneManager.UnloadSceneNameIndexInternal("", scene.buildIndex, true, options, out _);

            return false;
        }

        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}
*/