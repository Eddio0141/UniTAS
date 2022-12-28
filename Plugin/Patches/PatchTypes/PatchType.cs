using System;

namespace UniTASPlugin.Patches.PatchTypes;

public abstract class PatchType : Attribute
{
    public int Priority { get; }

    protected PatchType(int priority = 0)
    {
        Priority = priority;
    }
}