using System.Collections.Generic;
using Mono.Cecil;

namespace UniTAS.Patcher.Interfaces;

/// <summary>
/// Base class for all preload patchers
/// </summary>
public abstract class PreloadPatcher
{
    // keep in mind if inheriting from this class, you need to add your patcher to PreloadPatcherProcessor.cs
    public abstract IEnumerable<string> TargetDLLs { get; }

    public abstract void Patch(ref AssemblyDefinition assembly);
}