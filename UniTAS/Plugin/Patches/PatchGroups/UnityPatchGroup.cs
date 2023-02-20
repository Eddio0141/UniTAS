namespace UniTAS.Plugin.Patches.PatchGroups;

public class UnityPatchGroup : PatchGroup
{
    public string RangeStart { get; }
    public string RangeEnd { get; }

    public UnityPatchGroup(string rangeStart, string rangeEnd)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
    }

    public UnityPatchGroup()
    {
    }
}