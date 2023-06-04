using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Register]
[ForceInstantiate]
public class PatchHarmony
{
    public PatchHarmony(IEnumerable<Interfaces.Patches.PatchProcessor.PatchProcessor> patchProcessors, ILogger logger)
    {
        var harmony = new Harmony("dev.yuu0141.unitas");
        var sortedPatches = patchProcessors
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Key)
            .Select(x => x.Value);
        foreach (var patch in sortedPatches)
        {
            logger.LogDebug($"Patching {patch} via harmony");
            harmony.PatchAll(patch);
        }
    }
}