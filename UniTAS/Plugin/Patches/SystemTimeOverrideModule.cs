using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchGroups;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Utils;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Global

namespace UniTAS.Plugin.Patches;

[MscorlibPatch(true)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public class SystemTimeOverrideModule
{
    private static readonly IPatchReverseInvoker ReverseInvoker =
        Plugin.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

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
                if (ReverseInvoker.InnerCall())
                    return true;
                var gameTime = VirtualEnvironment.GameTime;
                __result = gameTime.CurrentTime;
                return false;
            }

            private static void Postfix()
            {
                ReverseInvoker.Return();
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
                __result = (int)(VirtualEnvironment.GameTime.RealtimeSinceStartup * 1000f);
                return false;
            }
        }
    }
}