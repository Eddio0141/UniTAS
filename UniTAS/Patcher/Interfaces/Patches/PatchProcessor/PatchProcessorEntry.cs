using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.Patches.PatchProcessor;

[RegisterAll(timing: RegisterTiming.Entry)]
public interface IPatchProcessorEntry
{
    /// <summary>
    /// Processes patch modules
    /// </summary>
    /// <returns>List of patch priority and classes that contains classes for patching</returns>
    public IEnumerable<(int, Type)> ProcessModules();
}