using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Plugin.Patches.PatchTypes;
using UniTAS.Plugin.Trackers.DontDestroyOnLoadTracker;
using UnityEngine;

namespace UniTAS.Plugin.Patches.RawPatches;

[RawPatch(1000)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class DontDestroyOnLoadTrackerPatch
{
    private static readonly IDontDestroyOnLoadTracker DontDestroyOnLoadTracker =
        Plugin.Kernel.GetInstance<IDontDestroyOnLoadTracker>();

    [HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
    private class DontDestroyOnLoadPatch
    {
        private static void Prefix(Object target)
        {
            DontDestroyOnLoadTracker.DontDestroyOnLoad(
                target is not Transform transform ? target : transform.gameObject);
        }
    }
}