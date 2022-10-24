using System;
using System.Reflection;
using HarmonyLib;
using Ninject;
using UniTASPlugin.GameEnvironment;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.UnityEngine.EventSystems;

[HarmonyPatch]
internal static class EventSystem
{
    private static class Helper
    {
        public static Type GetEventSystem()
        {
            return AccessTools.TypeByName("UnityEngine.EventSystems.EventSystem");
        }
    }

    [HarmonyPatch]
    internal class isFocusedGetter
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(Helper.GetEventSystem(), "isFocused");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!Plugin.Instance.Kernel.Get<VirtualEnvironment>().RunVirtualEnvironment) return true;
            __result = true;
            return false;
        }
    }
}