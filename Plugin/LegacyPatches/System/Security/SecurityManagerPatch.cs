using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.ReverseInvoker;

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.LegacyPatches.System.Security;

[HarmonyPatch]
internal static class SecurityManagerPatch
{
    [HarmonyPatch]
    private class EnsureElevatedPermissions
    {
        private static MethodBase TargetMethod()
        {
            var securityManagerType = AccessTools.TypeByName("System.Security.SecurityManager");
            return AccessTools.Method(securityManagerType, "EnsureElevatedPermissions");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking;
        }
    }
}