using System;
using System.Collections.Generic;
using HarmonyLib;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchGroups;
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

    /// Method to determine if the patch group should be used
    protected abstract IEnumerable<int> ChoosePatch(PatchType patchType, PatchGroup[] patchGroups);

    public void ProcessModules()
    {
        // TODO replace this later
        var pluginAssembly = typeof(Plugin).Assembly;

        // list of patch groups, patch group attributes, and PatchTypes for each modules
        var patchGroupsAttributesAndPatchType =
            new List<KeyValuePair<KeyValuePair<Type[], PatchGroup[]>, PatchType>>();
        foreach (var type in pluginAssembly.GetTypes())
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
                (PatchType)attributes[0]));
        }

        foreach (var patchGroupsAndPatchType in patchGroupsAttributesAndPatchType)
        {
            var patchType = patchGroupsAndPatchType.Value;
            var patchGroupsAndAttributes = patchGroupsAndPatchType.Key;

            var chosenIndexes = ChoosePatch(patchType, patchGroupsAndAttributes.Value);

            foreach (var chosenIndex in chosenIndexes)
            {
                var patchGroup = patchGroupsAndAttributes.Key[chosenIndex];

                _logger.LogInfo($"Applying patch group {patchGroup.FullName}");

                Harmony.CreateAndPatchAll(patchGroup);
            }
        }
    }
}