using System;
using JetBrains.Annotations;

namespace UniTAS.Patcher.Interfaces.Patches.PatchTypes;

[MeansImplicitUse]
public abstract class PatchTypeEntry(int priority = 0) : Attribute
{
    public int Priority { get; } = priority;
}