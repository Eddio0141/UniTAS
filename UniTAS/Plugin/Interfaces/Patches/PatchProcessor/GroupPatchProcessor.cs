using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchGroups;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Interfaces.Patches.PatchProcessor;

public abstract class GroupPatchProcessor : PatchProcessor
{
    private readonly ILogger _logger;

    protected GroupPatchProcessor(ILogger logger)
    {
        _logger = logger;
    }

    protected abstract Type TargetPatchType { get; }

    /// Method to determine if the patch group should be used
    protected abstract IEnumerable<int> ChoosePatch(ModulePatchType patchType, PatchGroup[] patchGroups);

    public override IEnumerable<KeyValuePair<int, Type>> ProcessModules()
    {
        // TODO replace this later
        var pluginTypes = AccessTools.GetTypesFromAssembly(typeof(Plugin).Assembly);

        // list of patch groups, patch group attributes, and PatchTypes for each modules
        var patchGroupsAttributesAndPatchType =
            new List<KeyValuePair<KeyValuePair<Type[], PatchGroup[]>, ModulePatchType>>();
        foreach (var type in pluginTypes)
        {
            var attributes = type.GetCustomAttributes(TargetPatchType, false);
            if (attributes.Length == 0) continue;

            _logger.LogInfo($"Found patch module {type.FullName}");

            var patchGroups = new List<Type>();
            var patchGroupAttributes = new List<PatchGroup>();

            foreach (var innerType in type.GetNestedTypes(AccessTools.all))
            {
                var innerAttributes = innerType.GetCustomAttributes(typeof(PatchGroup), false);
                if (innerAttributes.Length == 0) continue;

                patchGroups.Add(innerType);
                patchGroupAttributes.Add(innerAttributes[0] as PatchGroup);
            }

            patchGroupsAttributesAndPatchType.Add(new(new(patchGroups.ToArray(), patchGroupAttributes.ToArray()),
                (ModulePatchType)attributes[0]));
        }

        foreach (var patchGroupsAndPatchType in patchGroupsAttributesAndPatchType)
        {
            var patchType = patchGroupsAndPatchType.Value;
            var patchGroupsAndAttributes = patchGroupsAndPatchType.Key;

            var chosenIndexes = ChoosePatch(patchType, patchGroupsAndAttributes.Value);

            foreach (var chosenIndex in chosenIndexes)
            {
                var patchGroup = patchGroupsAndAttributes.Key[chosenIndex];

                _logger.LogInfo($"Using patch group {patchGroup.FullName}");

                foreach (var innerPatch in patchGroup.GetNestedTypes(AccessTools.all))
                {
                    yield return new(patchType.Priority, innerPatch);
                }
            }
        }
    }

    /// <summary>
    /// Helper function to convert a version string to a comparable integer
    /// </summary>
    /// <param name="version">Version number, all characters that is a . is removed, all non-numeric characters are set to 0</param>
    /// <param name="fallback">Fallback value in case the operation "fails"</param>
    /// <returns>A equal version number</returns>
    protected static ulong VersionStringToNumber(string version, ulong fallback = 0)
    {
        if (version == null) return fallback;

        version = version.Replace(".", "");

        // replace all non-numeric characters with 0
        var versionNumber = ulong.Parse(Regex.Replace(version, "[^0-9]", "0"));

        return versionNumber;
    }
}