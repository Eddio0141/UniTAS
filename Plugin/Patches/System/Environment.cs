using System;
using System.Reflection;
using HarmonyLib;
using Ninject;
using UniTASPlugin.FakeGameState;
using EnvOrig = System.Environment;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.System;

[HarmonyPatch]
internal static class Environment
{
    [HarmonyPatch(typeof(EnvOrig), nameof(EnvOrig.TickCount), MethodType.Getter)]
    private class get_TickCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (Plugin.Kernel.Get<PatchReverseInvoker>().Invoking)
                return true;
            __result = (int)(GameTime.RealtimeSinceStartup * 1000f);
            return false;
        }
    }
}