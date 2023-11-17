using System;
using JetBrains.Annotations;

namespace UniTAS.Patcher.Interfaces.Patches.PatchTypes;

[MeansImplicitUse]
public abstract class PatchType : Attribute
{
    public int Priority { get; }

    protected PatchType(int priority = 0)
    {
        Priority = priority;
    }
}