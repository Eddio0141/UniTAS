using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.ReverseInvoker;
using DateTimeOrig = System.DateTime;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTASPlugin.LegacyPatches.System;

[HarmonyPatch]
internal static class DateTimePatch
{
    [HarmonyPatch(typeof(DateTimeOrig), nameof(DateTimeOrig.Now), MethodType.Getter)]
    private class get_Now
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return true;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = gameTime.CurrentTime;
            return false;
        }
    }
}