using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Implementations.VirtualEnvironment;

namespace UniTAS.Patcher.Patches.Harmony;

// [RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public class EnvironmentPatch
{
    private static readonly VirtualEnvController VirtualEnvController =
        Plugin.Kernel.GetInstance<VirtualEnvController>();

    // [HarmonyPatch(typeof(Environment), "IsRunningOnWindows", MethodType.Getter)]
    // private class IsRunningOnWindows
    // {
    //     private static Exception Cleanup(MethodBase original, Exception ex)
    //     {
    //         return PatchHelper.CleanupIgnoreFail(original, ex);
    //     }
    //
    //     private static bool Prefix(ref bool __result)
    //     {
    //         // __result = VirtualEnvController.Os == Os.Windows;
    //         return false;
    //     }
    // }
}