using System;
using System.Collections.Generic;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchProcessor;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations.PatchProcessor;

// ReSharper disable once UnusedType.Global
public class RawPatchOnPluginInitProcessor : OnPluginInitProcessor
{
    private readonly ILogger _logger;

    public RawPatchOnPluginInitProcessor(ILogger logger)
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
            var attributes = type.GetCustomAttributes(typeof(RawPatchOnPluginInit), false);
            if (attributes.Length == 0) continue;

            var rawPatch = (RawPatchOnPluginInit)attributes[0];

            _logger.LogInfo($"Found raw patch module for plugin init patch {type.FullName}");

            foreach (var innerType in type.GetNestedTypes(AccessTools.all))
            {
                var innerTypeHarmonyPatch = innerType.GetCustomAttributes(typeof(HarmonyPatch), false);
                if (innerTypeHarmonyPatch.Length == 0) continue;
                yield return new(rawPatch.Priority, innerType);
            }
        }
    }
}