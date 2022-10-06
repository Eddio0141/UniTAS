using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState;

namespace UniTASPlugin.Patches.__System;

[HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
class get_Now
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref DateTime __result)
    {
        __result = GameTime.CurrentTime;
        return false;
    }
}
