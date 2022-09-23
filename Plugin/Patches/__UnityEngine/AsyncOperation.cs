using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

// TODO different unity version investigation
[HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Setter)]
class setAllowSceneActivation
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(bool value, AsyncOperation __instance)
    {
        Plugin.Log.LogDebug($"allowSceneActivation set to {value} at {FakeGameState.GameTime.FrameCount}");
        GameTracker.AllowSceneActivation(value, __instance);
    }
}

[HarmonyPatch(typeof(AsyncOperation), "Finalize")]
class Finialze
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static bool Prefix(AsyncOperation __instance)
    {
        // we skip the internal destruction if its "fake"
        if (GameTracker.DestroyAsyncOperation(__instance))
            return false;
        return true;
    }
}