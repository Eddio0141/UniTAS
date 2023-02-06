using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.ReverseInvoker;

namespace UniTASPlugin.Patches.RawPatches;

// [RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class EnvironmentPatch
{
    private static readonly IReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<ReverseInvokerFactory>();

    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

    [HarmonyPatch(typeof(Environment), "IsRunningOnWindows", MethodType.Getter)]
    private class IsRunningOnWindows
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;

            __result = VirtualEnvironment.Os == Os.Windows;

            return false;
        }
    }
}