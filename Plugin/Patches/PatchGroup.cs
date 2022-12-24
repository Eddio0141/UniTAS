using System;

namespace UniTASPlugin.Patches;

public class PatchGroup : Attribute
{
    public string RangeStart { get; }
    public string RangeEnd { get; }

    public PatchGroup(string rangeStart, string rangeEnd)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
    }

    public PatchGroup(string version) : this(version, version)
    {
    }
}