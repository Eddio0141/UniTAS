using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Patches.PatchGroups;

namespace UniTAS.Plugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
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
                FileSystemManager.GetTempPath(out __result);

                return false;
            }
        }
    }
}