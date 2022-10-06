using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.Patches.__System.__Security;

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
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            return false;
        }
    }
}
