using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Logger;

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

        var patchGroups = new List<Type>();
        foreach (var type in pluginAssembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(TargetPatchType, false);
            if (attributes.Length == 0) continue;

            foreach (var innerType in type.GetNestedTypes())
            {
                var patchGroupAttributes = innerType.GetCustomAttributes(typeof(PatchGroup), false);
                if (patchGroupAttributes.Length == 0) continue;

                patchGroups.Add(innerType);
            }
        }

        var version = VersionStringToNumber(Version);
        Type chosenPatch = null;
        var chosenRangeStart = 0ul;
        var chosenRangeEnd = 0ul;

        foreach (var patchGroup in patchGroups)
        {
            var foundAttribute = patchGroup.GetCustomAttributes(typeof(PatchGroup), false).First();
            if (foundAttribute is not PatchGroup patchGroupAttribute) continue;
            var versionStart = VersionStringToNumber(patchGroupAttribute.RangeStart);
            var versionEnd = VersionStringToNumber(patchGroupAttribute.RangeEnd);

            if (chosenPatch == null)
            {
                chosenPatch = patchGroup;
                chosenRangeStart = versionStart;
                chosenRangeEnd = versionEnd;
                continue;
            }

            if (versionStart <= version && version <= versionEnd)
            {
                chosenPatch = patchGroup;
                break;
            }

            // prioritise the patch group that is closest to the current version, but lower than version

            if (versionEnd < version && versionEnd > chosenRangeEnd)
            {
                chosenPatch = patchGroup;
                chosenRangeStart = versionStart;
                chosenRangeEnd = versionEnd;
                continue;
            }

            if (versionStart > version && versionStart < chosenRangeStart)
            {
                chosenPatch = patchGroup;
                chosenRangeStart = versionStart;
                chosenRangeEnd = versionEnd;
            }
        }

        if (chosenPatch == null)
        {
            return;
        }

        // patch everything in the chosen patch group
        // bepInEx will take care of the rest
        foreach (var patchType in chosenPatch.GetNestedTypes())
        {
            _logger.LogDebug($"Attempting patch to {patchType.Name}");
            HarmonyLib.Harmony.CreateAndPatchAll(patchType);
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