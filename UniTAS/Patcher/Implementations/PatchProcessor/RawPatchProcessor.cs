using System;
using System.Collections.Generic;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.MonoBehaviourScripts;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.PatchProcessor;

// ReSharper disable once UnusedType.Global
public class RawPatchProcessor : Interfaces.Patches.PatchProcessor.PatchProcessor
{
    private readonly ILogger _logger;

    public RawPatchProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public override IEnumerable<(int, Type)> ProcessModules()
    {
        var pluginTypes = AccessTools.GetTypesFromAssembly(typeof(MonoBehaviourUpdateInvoker).Assembly);

        // list of patch groups, patch group attributes, and PatchTypes for each modules
        foreach (var type in pluginTypes)
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