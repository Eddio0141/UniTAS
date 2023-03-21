using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Shared;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[HarmonyPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DontDestroyOnLoadTracker
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

            var obj = target switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };

            if (obj == null)
            {
                Trace.Write($"DontDestroyOnLoad target is neither GameObject nor Component, ignoring");
                return;
            }

            // check if root
            if (obj.transform.parent != null)
            {
                Trace.Write($"DontDestroyOnLoad target is not root, ignoring");
                return;
            }

            Trace.Write($"DontDestroyOnLoad target is root, adding to tracker");
            Tracker.DontDestroyGameObjects.Add(obj);
        }
    }
}