using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.Unity;

[HarmonyPatch(typeof(Object))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ObjectPatch
{
    [HarmonyPatch(nameof(Object.DontDestroyOnLoad))]
    private class DontDestroyOnLoadPatch
    {
        private static void Prefix(object obj)
        {
            Trace.Write("DontDestroyOnLoad called!");
        }
    }
}