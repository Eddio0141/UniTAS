namespace UniTASPlugin.Patches.PatchGroups;

public class MscorlibPatchGroup : PatchGroup
{
    public string RangeStart { get; }
    public string RangeEnd { get; }
    public bool NetStandard21 { get; }

    public MscorlibPatchGroup(string rangeStart, string rangeEnd, bool netStandard21)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        NetStandard21 = netStandard21;
    }

    public MscorlibPatchGroup()
    {
    }
}