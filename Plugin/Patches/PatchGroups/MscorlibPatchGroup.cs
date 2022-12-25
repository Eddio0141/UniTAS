namespace UniTASPlugin.Patches.PatchGroups;

public class MscorlibPatchGroup : PatchGroup
{
    public string RangeStart { get; }
    public string RangeEnd { get; }
    public string NetStandardVersion { get; }

    public MscorlibPatchGroup(string rangeStart, string rangeEnd, string netStandardVersion)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        NetStandardVersion = netStandardVersion;
    }

    public MscorlibPatchGroup(string rangeEnd) : this(null, rangeEnd, null)
    {
    }

    public MscorlibPatchGroup()
    {
    }
}