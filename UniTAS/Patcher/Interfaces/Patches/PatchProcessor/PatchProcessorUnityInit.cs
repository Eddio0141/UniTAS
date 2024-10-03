using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.Patches.PatchProcessor;

[RegisterAll]
public interface IPatchProcessorUnityInit
{
    /// <summary>
    /// Processes patch modules
    /// </summary>
    /// <returns>List of patch priority and classes that contains classes for patching</returns>
    public IEnumerable<(int, Type)> ProcessModules();
}