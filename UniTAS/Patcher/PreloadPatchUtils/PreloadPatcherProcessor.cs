using System;
using System.Linq;

namespace UniTAS.Patcher.PreloadPatchUtils;

public class PreloadPatcherProcessor
{
    public PreloadPatcher[] PreloadPatchers { get; }
    public string[] TargetDLLs => PreloadPatchers.SelectMany(p => p.TargetDLLs).Distinct().ToArray();

    public PreloadPatcherProcessor()
    {
        var currentAssembly = typeof(PreloadPatcherProcessor).Assembly;
        PreloadPatchers = currentAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PreloadPatcher)))
            .Select(t => (PreloadPatcher)Activator.CreateInstance(t))
            .ToArray();
    }
}