using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.StaticServices;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DontDestroyOnLoadTracker
{
    [HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
    private class DontDestroyOnLoadPatch
    {
        private static readonly List<string> _initialExcludeNames = new()
        {
            "BepInEx_Manager",
            "BepInEx_ThreadingHelper",
            ManagerGameObject.GameObjectName
        };

        private static void Prefix(Object target)
        {
            if (_initialExcludeNames.Contains(target.name))
            {
                Entry.Logger.LogDebug($"Ignoring initial DontDestroyOnLoad tracking for {target.name}");
                _initialExcludeNames.Remove(target.name);
                return;
            }

            Entry.Logger.LogDebug(
                $"DontDestroyOnLoad invoked, target name: {target.name}, target type: {target.GetType()}");

            var obj = target switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };

            if (obj == null)
            {
                Entry.Logger.LogDebug($"DontDestroyOnLoad target is neither GameObject nor Component, ignoring");
                return;
            }

            // check if root
            if (obj.transform.parent != null)
            {
                Entry.Logger.LogDebug($"DontDestroyOnLoad target is not root, ignoring");
                return;
            }

            Entry.Logger.LogDebug($"DontDestroyOnLoad target is root, adding to tracker");
            Tracker.DontDestroyGameObjects.Add(obj);
        }
    }
}