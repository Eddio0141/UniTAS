using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Patches.PatchGroups;
using UniTASPlugin.Patches.PatchTypes;
using UniTASPlugin.ReverseInvoker;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Global

namespace UniTASPlugin.Patches.Modules;

[MscorlibPatch(true)]
public class SystemTimeOverrideModule
{
    private static readonly ReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<ReverseInvokerFactory>();

    private static readonly IVirtualEnvironmentFactory VirtualEnvironmentFactory =
        Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>();

    [MscorlibPatchGroup]
    private class AllVersions
    {
        [HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
        private class get_Now
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix(ref DateTime __result)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
                __result = gameTime.CurrentTime;
                return false;
            }
        }

        [HarmonyPatch(typeof(Environment), nameof(Environment.TickCount), MethodType.Getter)]
        private class get_TickCount
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix(ref int __result)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
                __result = (int)(gameTime.RealtimeSinceStartup * 1000f);
                return false;
            }
        }
    }
}