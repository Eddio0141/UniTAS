using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.Patches.PatchGroups;

namespace UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public partial class PathPatchModule
{
    [MscorlibPatchGroup("3.9.9.9")]
    private class Pre4000
    {
        [HarmonyPatch]
        private class get_temp_path
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(Path), "get_temp_path");
            }

            private static bool Prefix(ref string __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.GetTempPath(out __result);

                return false;
            }
        }
    }
}