using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.Patches.PatchProcessor;

[RegisterAll]
public abstract class PatchProcessor
{
    /// <summary>
    /// Processes patch modules
    /// </summary>
    /// <returns>List of patch priority and types for patching</returns>
    public abstract IEnumerable<KeyValuePair<int, Type>> ProcessModules();
}