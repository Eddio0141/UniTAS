using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.MonoBehaviourScripts;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Register]
public class PatchHarmony
{
    public PatchHarmony(IEnumerable<Interfaces.Patches.PatchProcessor.PatchProcessor> patchProcessors, ILogger logger)
    {
        var sortedPatches = patchProcessors
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Key)
            .Select(x => x.Value);
        foreach (var patch in sortedPatches)
        {
            logger.LogDebug($"Patching {patch} via harmony");
            MonoBehaviourUpdateInvoker.Harmony.PatchAll(patch);
        }
    }
}