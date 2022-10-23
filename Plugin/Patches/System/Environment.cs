using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.FakeGameState;
using EnvOrig = System.Environment;

namespace UniTASPlugin.Patches.System;

[HarmonyPatch]
static class Environment
{
    [HarmonyPatch(typeof(EnvOrig), nameof(EnvOrig.TickCount), MethodType.Getter)]
    class get_TickCount
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result)
        {
            __result = (int)(GameTime.RealtimeSinceStartup * 1000f);
            return false;
        }
    }
}