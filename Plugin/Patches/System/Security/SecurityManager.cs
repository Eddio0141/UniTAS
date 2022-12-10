using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.ReverseInvoker;

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.System.Security;

[HarmonyPatch]
internal static class SecurityManager
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
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking;
        }
    }
}