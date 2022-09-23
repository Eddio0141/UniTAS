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

    static void Prefix(bool value)
    {
        Plugin.Log.LogDebug($"allowSceneActivation set to {value} at {FakeGameState.GameTime.FrameCount}");
        GameTracker.AllowSceneActivation(value);
    }
}