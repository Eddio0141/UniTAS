using System;
using System.Collections.Generic;

namespace UniTASPlugin.Patches.PatchProcessor;

public abstract class PatchProcessor
{
    /// <summary>
    /// Processes patch modules
    /// </summary>
    /// <returns>List of patch priority and types for patching</returns>
    public abstract IEnumerable<KeyValuePair<int, Type>> ProcessModules();
}