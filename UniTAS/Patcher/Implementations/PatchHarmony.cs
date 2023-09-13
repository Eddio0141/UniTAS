using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class PatchHarmony
{
    public PatchHarmony(IHarmony harmony, IEnumerable<Interfaces.Patches.PatchProcessor.PatchProcessor> patchProcessors,
        ILogger logger)
    {
        var sortedPatches = patchProcessors
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Item1)
            .Select(x => x.Item2);
        foreach (var patch in sortedPatches)
        {
            logger.LogDebug($"Patching {patch} via harmony");
            harmony.Harmony.PatchAll(patch);
        }
    }
}