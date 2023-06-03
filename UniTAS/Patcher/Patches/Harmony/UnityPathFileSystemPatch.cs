using System.Diagnostics.CodeAnalysis;

namespace UniTAS.Patcher.Patches.Harmony;

// [RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class UnityPathFileSystemPatch
{
    // TODO add all path patches
    // private static readonly PatchReverseInvoker
    //     ReverseInvoker = Plugin.Kernel.GetInstance<PatchReverseInvoker>();
    //
    // private static readonly VirtualEnvController VirtualEnvController =
    //     Plugin.Kernel.GetInstance<VirtualEnvController>();
    //
    // [HarmonyPatch(typeof(Application), nameof(Application.persistentDataPath), MethodType.Getter)]
    // private class ApplicationPersistentDataPath
    // {
    //     private static Exception Cleanup(MethodBase original, Exception ex)
    //     {
    //         return PatchHelper.CleanupIgnoreFail(original, ex);
    //     }
    //
    //     private static bool Prefix(ref string __result)
    //     {
    //         if (ReverseInvoker.InnerCall()) return true;
    //
    //         __result = VirtualEnvController.UnityPathsEnv.PersistentDataPath;
    //
    //         return false;
    //     }
    //
    //     private static void Postfix()
    //     {
    //         ReverseInvoker.Return();
    //     }
    // }
}