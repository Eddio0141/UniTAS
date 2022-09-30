using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState;

namespace UniTASPlugin.Patches.__System;

[HarmonyPatch(typeof(Environment), nameof(Environment.TickCount), MethodType.Getter)]
class get_TickCount
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        __result = (int)(GameTime.RealtimeSinceStartup * 1000f);
        return false;
    }
}
