using System.Collections.Generic;
using Mono.Cecil;

namespace UniTAS.Patcher.Interfaces;

/// <summary>
/// Base class for all preload patchers
/// </summary>
public abstract class PreloadPatcher
{
    public abstract IEnumerable<string> TargetDLLs { get; }

    public abstract void Patch(ref AssemblyDefinition assembly);
}