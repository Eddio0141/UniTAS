using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Patches.Preloader;

namespace UniTAS.Patcher.Implementations;

public class PreloadPatcherProcessor
{
    public PreloadPatcher[] PreloadPatchers { get; } =
    [
        new MonoBehaviourPatch(),
        new StaticCtorHeaders(),
        new UnityInitInvoke(),
        new FinalizeSuppressionPatch(),
        new FunctionCallTrace()
    ];

    public IEnumerable<string> TargetDLLs => PreloadPatchers.SelectMany(p => p.TargetDLLs).Distinct().ToArray();
}