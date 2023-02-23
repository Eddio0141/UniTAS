using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Runtime;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.Unity;

[HarmonyPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ObjectPatch
{
    [HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
    private class DontDestroyOnLoadPatch
    {
        private static readonly List<string> _initialExcludeNames = new()
        {
            "BepInEx_Manager",
            "BepInEx_ThreadingHelper"
        };

        private static void Prefix(Object target)
        {
            if (_initialExcludeNames.Contains(target.name))
            {
                Trace.Write($"Ignoring initial DontDestroyOnLoad tracking for {target.name}");
                _initialExcludeNames.Remove(target.name);
                return;
            }

            Trace.Write(
                $"DontDestroyOnLoad invoked, target name: {target.name}, target type: {target.GetType()}");
            Tracker.DontDestroyOnLoadObjects.Add(target);
        }
    }
}