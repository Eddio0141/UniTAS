using System;

namespace UniTASPlugin.Patches.PatchTypes;

public abstract class PatchType : Attribute
{
    /// <summary>
    /// Patches all patch groups that meets the patch group requirements
    /// </summary>
    public bool PatchAllGroups { get; }

    protected PatchType(bool patchAllGroups)
    {
        PatchAllGroups = patchAllGroups;
    }

    protected PatchType() : this(false)
    {
    }
}