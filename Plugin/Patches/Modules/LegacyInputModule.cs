using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Patches.PatchGroups;
using UniTASPlugin.Patches.PatchTypes;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.Modules;

[UnityPatch(true)]
public class LegacyInputModule
{
    private static readonly IReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<IReverseInvokerFactory>();

    private static readonly IVirtualEnvironmentFactory VirtualEnvironmentFactory =
        Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>();

    [UnityPatchGroup]
    private class AllVersions
    {
        // gets called from GetKey
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyInt))]
        private class GetKeyInt
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix(object key, ref bool __result)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                if (!env.RunVirtualEnvironment) return true;
                __result = env.InputState.KeyboardState.Keys.Contains((int)(KeyCode)key);
                return false;
            }
        }

        // gets called from GetKey
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyString))]
        private class GetKeyString
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix( /*string name, ref bool __result*/)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                return !env.RunVirtualEnvironment;
                // TODO
            }
        }

        // gets called from GetKeyUp
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpString))]
        private class GetKeyUpString
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix( /*string name, ref bool __result*/)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                return !env.RunVirtualEnvironment;
                // TODO
            }
        }

        // gets called from GetKeyUp
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpInt))]
        private class GetKeyUpInt
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix(object key, ref bool __result)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                if (!env.RunVirtualEnvironment) return true;
                __result = env.InputState.KeyboardState.KeysUp.Contains((int)(KeyCode)key);
                return false;
            }
        }

        // gets called from GetKeyDown
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownString))]
        private class GetKeyDownString
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix( /*string name*/)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                return !env.RunVirtualEnvironment;
                // TODO
            }
        }

        // gets called from GetKeyDown
        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownInt))]
        private class GetKeyDownInt
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static bool Prefix(object key, ref bool __result)
            {
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    return true;
                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                if (!env.RunVirtualEnvironment) return true;
                __result = env.InputState.KeyboardState.KeysDown.Contains((int)(KeyCode)key);
                return false;
            }
        }
    }
}