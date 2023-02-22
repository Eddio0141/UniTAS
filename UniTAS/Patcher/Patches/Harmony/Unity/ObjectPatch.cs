using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.Unity;

[HarmonyPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ObjectPatch
{
    [HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
    private class DontDestroyOnLoadPatch
    {
        private static void Prefix(Object target)
        {
            Trace.Write($"DontDestroyOnLoad called!, target name: {target.name}, target type: {target.GetType()}");
        }
    }
}