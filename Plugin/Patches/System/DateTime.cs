using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.FakeGameState;
using DateTimeOrig = System.DateTime;

namespace UniTASPlugin.Patches.System;

[HarmonyPatch]
static class DateTime
{
    [HarmonyPatch(typeof(DateTimeOrig), nameof(DateTimeOrig.Now), MethodType.Getter)]
    class get_Now
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DateTimeOrig __result)
        {
            __result = GameTime.CurrentTime;
            return false;
        }
    }
}
