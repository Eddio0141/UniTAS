using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Plugin.Patches.PatchTypes;
using UnityEngine;

namespace UniTAS.Plugin.Patches.RawPatches;

[RawPatch(1000)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class DontDestroyOnLoadTrackerPatch
{
    // [HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
    // private class DontDestroyOnLoadPatch
    // {
    //     private static void Prefix(Object target)
    //     {
    //         Trace.Write($"DontDestroyOnLoad called from plugin, target name: {target.name}, target type: {target.GetType()}");
    //     }
    // }
}