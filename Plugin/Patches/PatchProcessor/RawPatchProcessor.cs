using System;
using System.Collections.Generic;
using HarmonyLib;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchTypes;

namespace UniTASPlugin.Patches.PatchProcessor;

// ReSharper disable once UnusedType.Global
public class RawPatchProcessor : PatchProcessor
{
    private readonly ILogger _logger;

    public RawPatchProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public override IEnumerable<KeyValuePair<int, Type>> ProcessModules()
    {
        // TODO replace this later
        var pluginAssembly = typeof(Plugin).Assembly;

        // list of patch groups, patch group attributes, and PatchTypes for each modules
        foreach (var type in pluginAssembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes(typeof(RawPatch), false);
            if (attributes.Length == 0) continue;

            var rawPatch = (RawPatch)attributes[0];

            _logger.LogInfo($"Found raw patch module {type.FullName}");

            foreach (var innerType in type.GetNestedTypes(AccessTools.all))
            {
                var innerTypeHarmonyPatch = innerType.GetCustomAttributes(typeof(HarmonyPatch), false);
                if (innerTypeHarmonyPatch.Length == 0) continue;
                yield return new(rawPatch.Priority, innerType);
            }
        }
    }
}