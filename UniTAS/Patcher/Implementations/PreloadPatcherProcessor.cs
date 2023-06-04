using System;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces;

namespace UniTAS.Patcher.Implementations;

public class PreloadPatcherProcessor
{
    public PreloadPatcher[] PreloadPatchers { get; }
    public string[] TargetDLLs => PreloadPatchers.SelectMany(p => p.TargetDLLs).Distinct().ToArray();

    public PreloadPatcherProcessor()
    {
        var currentAssembly = typeof(PreloadPatcherProcessor).Assembly;
        PreloadPatchers = AccessTools.GetTypesFromAssembly(currentAssembly)
            .Where(t => t.IsSubclassOf(typeof(PreloadPatcher)))
            .Select(t => (PreloadPatcher)Activator.CreateInstance(t))
            .ToArray();
    }
}