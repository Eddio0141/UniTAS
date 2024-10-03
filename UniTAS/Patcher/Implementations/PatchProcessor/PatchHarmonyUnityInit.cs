using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.PatchProcessor;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class PatchHarmonyUnityInit
{
    public PatchHarmonyUnityInit(IHarmony harmony,
        IEnumerable<Interfaces.Patches.PatchProcessor.IPatchProcessorUnityInit> patchProcessors,
        ILogger logger)
    {
        var sortedPatches = patchProcessors
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Item1)
            .Select(x => x.Item2);
        foreach (var patch in sortedPatches)
        {
            logger.LogDebug($"Patching {patch} via harmony (UnityInit)");
            harmony.Harmony.PatchAll(patch);
        }
    }
}
