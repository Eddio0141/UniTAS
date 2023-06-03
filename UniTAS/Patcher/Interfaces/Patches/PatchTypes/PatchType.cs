using System;

namespace UniTAS.Patcher.Interfaces.Patches.PatchTypes;

public abstract class PatchType : Attribute
{
    public int Priority { get; }

    protected PatchType(int priority = 0)
    {
        Priority = priority;
    }
}