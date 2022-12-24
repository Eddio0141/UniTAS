using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchTypes;

namespace UniTASPlugin.Patches.PatchProcessor;

public abstract class PatchProcessor
{
    private readonly ILogger _logger;

    protected PatchProcessor(ILogger logger)
    {
        _logger = logger;
    }

    protected abstract Type TargetPatchType { get; }
    protected abstract string Version { get; }

    public void ProcessModules()
    {
        // TODO replace this later
        var pluginAssembly = typeof(Plugin).Assembly;

        // list of patch groups, and PatchType
        var targetModulesAndPatchGroups = new List<KeyValuePair<List<Type>, PatchType>>();
        foreach (var type in pluginAssembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(TargetPatchType, false);
            if (attributes.Length == 0) continue;

            var patchType = (PatchType)attributes[0];
            var patchGroups = new List<Type>();

            foreach (var innerType in type.GetNestedTypes())
            {
                if (innerType.GetCustomAttributes(typeof(PatchGroup), false).Length == 0) continue;

                patchGroups.Add(innerType);
            }

            targetModulesAndPatchGroups.Add(new(patchGroups, patchType));
        }

        var version = VersionStringToNumber(Version);

        foreach (var targetModulesAndPatchGroup in targetModulesAndPatchGroups)
        {
            var patchGroups = targetModulesAndPatchGroup.Key;
            var patchType = targetModulesAndPatchGroup.Value;

            // different process
            if (patchType is { PatchAllGroups: true })
            {
                foreach (var group in patchGroups)
                {
                    PatchAllInGroup(group);
                }

                continue;
            }

            Type chosenGroup = null;
            var chosenRangeStart = 0ul;
            var chosenRangeEnd = 0ul;

            foreach (var patchGroup in patchGroups)
            {
                var patchGroupAttribute = (PatchGroup)patchGroup.GetCustomAttributes(typeof(PatchGroup), false).First();
                var versionStart = VersionStringToNumber(patchGroupAttribute.RangeStart);
                var versionEnd = VersionStringToNumber(patchGroupAttribute.RangeEnd);

                if (chosenGroup == null)
                {
                    chosenGroup = patchGroup;
                    chosenRangeStart = versionStart;
                    chosenRangeEnd = versionEnd;
                    continue;
                }

                if (versionStart <= version && version <= versionEnd)
                {
                    chosenGroup = patchGroup;
                    break;
                }

                // prioritise the patch group that is closest to the current version, but lower than version

                if (versionEnd < version && versionEnd > chosenRangeEnd)
                {
                    chosenGroup = patchGroup;
                    chosenRangeStart = versionStart;
                    chosenRangeEnd = versionEnd;
                    continue;
                }

                if (versionStart > version && versionStart < chosenRangeStart)
                {
                    chosenGroup = patchGroup;
                    chosenRangeStart = versionStart;
                    chosenRangeEnd = versionEnd;
                }
            }

            if (chosenGroup == null) continue;

            // patch everything in the chosen patch group
            PatchAllInGroup(chosenGroup);
        }
    }

    private void PatchAllInGroup(Type patchGroup)
    {
        foreach (var patch in patchGroup.GetNestedTypes())
        {
            _logger.LogDebug($"Attempting patch for {patch.Name}");
            HarmonyLib.Harmony.CreateAndPatchAll(patch);
        }
    }

    private static ulong VersionStringToNumber(string version)
    {
        var versionParts = version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        var versionNumber = 0ul;
        var multiplier = (ulong)Math.Pow(10, versionParts.Length - 1);
        foreach (var versionPart in versionParts)
        {
            if (!ulong.TryParse(versionPart, out var versionPartNumber))
            {
                versionPartNumber = 0;
            }

            versionNumber += versionPartNumber * multiplier;
            multiplier /= 10;
        }

        return versionNumber;
    }
}