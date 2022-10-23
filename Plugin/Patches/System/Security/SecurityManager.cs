using System;
using System.Reflection;
using HarmonyLib;

namespace UniTASPlugin.Patches.System.Security;

[HarmonyPatch]
static class SecurityManager
{
    [HarmonyPatch]
    class EnsureElevatedPermissions
    {
        static MethodBase TargetMethod()
        {
            var securityManagerType = AccessTools.TypeByName("System.Security.SecurityManager");
            return AccessTools.Method(securityManagerType, "EnsureElevatedPermissions");
        }

        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            return false;
        }
    }
}
