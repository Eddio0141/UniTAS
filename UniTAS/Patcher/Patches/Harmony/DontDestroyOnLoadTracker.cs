using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DontDestroyOnLoadTracker
{
    private static readonly ILogger _logger = ContainerStarter.Kernel.GetInstance<ILogger>();

    private static readonly IObjectTrackerUpdate _objectTracker =
        ContainerStarter.Kernel.GetInstance<IObjectTrackerUpdate>();

    [HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
    private class DontDestroyOnLoadPatch
    {
        private static readonly List<string> _initialExcludeNames = new()
        {
            "BepInEx_Manager",
            "BepInEx_ThreadingHelper",
            InitManagerGameObject.GameObjectName
        };

        private static void Prefix(Object target)
        {
            if (_initialExcludeNames.Contains(target.name))
            {
                _logger.LogDebug($"Ignoring initial DontDestroyOnLoad tracking for {target.name}");
                _initialExcludeNames.Remove(target.name);
                return;
            }

            _logger.LogDebug(
                $"DontDestroyOnLoad invoked, target name: {target.name}, target type: {target.GetType()}");

            var obj = target switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };

            if (obj == null)
            {
                _logger.LogDebug($"DontDestroyOnLoad target is neither GameObject nor Component, ignoring");
                return;
            }

            // check if root
            if (obj.transform.parent != null)
            {
                _logger.LogDebug($"DontDestroyOnLoad target is not root, ignoring");
                return;
            }

            _logger.LogDebug($"DontDestroyOnLoad target is root, adding to tracker");
            _objectTracker.DontDestroyOnLoadAddRoot(obj);
        }
    }
}