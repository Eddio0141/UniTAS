using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine.__SceneManagment;

static class Helper
{
    public static Type GetSceneManager()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");
    }

    public static Type GetLoadSceneParameters()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");
    }

    public static Type GetScene()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");
    }

    public static Traverse GetLoadSceneAsyncNameIndexInternal()
    {
        return Traverse.Create(GetSceneManager()).Method("LoadSceneAsyncNameIndexInternal", new Type[] { typeof(string), typeof(int), GetLoadSceneParameters(), typeof(bool) });
    }

    public static Traverse GetUnloadSceneNameIndexInternal()
    {
        return Traverse.Create(GetSceneManager()).Method("UnloadSceneNameIndexInternal", new Type[] { typeof(string), typeof(int), typeof(bool), GetUnloadSceneOptions(), typeof(bool) });
    }

    public static Type GetUnloadSceneOptions()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.UnloadSceneOptions");
    }
}

[HarmonyPatch]
class UnloadSceneAsync__sceneBuildIndex
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsync", new Type[] { typeof(int) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(int sceneBuildIndex, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetUnloadSceneNameIndexInternal();
            var unloadSceneOptions = Helper.GetUnloadSceneOptions();
            var noneVariant = Enum.Parse(unloadSceneOptions, "None");
            __result = (AsyncOperation)internalCall.GetValue(new object[] { "", sceneBuildIndex, true, noneVariant, null });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch]
class LoadSceneAsync__sceneBuildIndex__parameters
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsync", new Type[] { typeof(int), Helper.GetLoadSceneParameters() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(int sceneBuildIndex, object parameters, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetLoadSceneAsyncNameIndexInternal();
            __result = (AsyncOperation)internalCall.GetValue(new object[] { null, sceneBuildIndex, parameters, true });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneLoad(__result);
    }
}

[HarmonyPatch]
class LoadSceneAsync__sceneName__parameters
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsync", new Type[] { typeof(string), Helper.GetLoadSceneParameters() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string sceneName, object parameters, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetLoadSceneAsyncNameIndexInternal();
            __result = (AsyncOperation)internalCall.GetValue(new object[] { sceneName, -1, parameters, true });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneLoad(__result);
    }
}

[HarmonyPatch]
class UnloadSceneAsync__sceneName
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsync", new Type[] { typeof(string) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string sceneName, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetUnloadSceneNameIndexInternal();
            var unloadSceneOptions = Helper.GetUnloadSceneOptions();
            var noneVariant = Enum.Parse(unloadSceneOptions, "None");
            __result = (AsyncOperation)internalCall.GetValue(new object[] { sceneName, -1, true, noneVariant, null });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch]
class UnloadSceneAsync__scene
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsync", new Type[] { Helper.GetScene() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref object scene, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetUnloadSceneNameIndexInternal();
            var sceneTraverse = Traverse.Create(scene);
            var unloadSceneOptions = Helper.GetUnloadSceneOptions();
            var noneVariant = Enum.Parse(unloadSceneOptions, "None");
            __result = (AsyncOperation)internalCall.GetValue(new object[] { "", sceneTraverse.Field("buildIndex").GetValue(), true, noneVariant, null });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch]
class UnloadSceneAsync__sceneBuildIndex__options
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsync", new Type[] { typeof(int), Helper.GetUnloadSceneOptions() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(int sceneBuildIndex, object options, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetUnloadSceneNameIndexInternal();
            __result = (AsyncOperation)internalCall.GetValue(new object[] { "", sceneBuildIndex, true, options, null });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch]
class UnloadSceneAsync__sceneName__options
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsync", new Type[] { typeof(string), Helper.GetUnloadSceneOptions() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string sceneName, object options, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetUnloadSceneNameIndexInternal();
            __result = (AsyncOperation)internalCall.GetValue(new object[] { sceneName, -1, true, options, null });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}

[HarmonyPatch]
class UnloadSceneAsync__scene__options
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsync", new Type[] { Helper.GetScene(), Helper.GetUnloadSceneOptions() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref object scene, object options, ref AsyncOperation __result)
    {
        if (TAS.Main.Running)
        {
            var internalCall = Helper.GetUnloadSceneNameIndexInternal();
            var sceneTraverse = Traverse.Create(scene);
            __result = (AsyncOperation)internalCall.GetValue(new object[] { "", sceneTraverse.Field("buildIndex").GetValue(), true, options, null });
            return false;
        }
        return true;
    }

    static void Postfix(ref AsyncOperation __result)
    {
        UnityASyncHandler.AsyncSceneUnload(__result);
    }
}