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
    /// <returns>List of patch priority and classes that contains classes for patching</returns>
    public abstract IEnumerable<(int, Type)> ProcessModules();
}