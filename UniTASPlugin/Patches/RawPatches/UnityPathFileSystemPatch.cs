using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.LegacyPatches;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

namespace UniTASPlugin.Patches.RawPatches;

// [RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class UnityPathFileSystemPatch
{
    // TODO add all path patches
    private static readonly PatchReverseInvoker
        ReverseInvoker = Plugin.Kernel.GetInstance<PatchReverseInvoker>();

    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

    [HarmonyPatch(typeof(Application), nameof(Application.persistentDataPath), MethodType.Getter)]
    private class ApplicationPersistentDataPath
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref string __result)
        {
            if (ReverseInvoker.InnerCall()) return true;

            __result = VirtualEnvironment.UnityPaths.PersistentDataPath;

            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }
}