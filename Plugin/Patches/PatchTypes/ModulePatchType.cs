namespace UniTASPlugin.Patches.PatchTypes;

public class ModulePatchType : PatchType
{
    /// <summary>
    /// Patches all patch groups that meets the patch group requirements
    /// </summary>
    public bool PatchAllGroups { get; }

    protected ModulePatchType(bool patchAllGroups)
    {
        PatchAllGroups = patchAllGroups;
    }

    protected ModulePatchType() : this(false)
    {
    }
}