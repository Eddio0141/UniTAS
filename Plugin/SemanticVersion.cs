using System;
using System.Collections.Generic;
using System.Linq;

namespace UniTASPlugin;

public class SemanticVersion
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }

    public SemanticVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public static bool operator <(SemanticVersion a, SemanticVersion b)
    {
        return a.Major < b.Major || a.Major == b.Major && a.Minor < b.Minor || a.Major == b.Major && a.Minor == b.Minor && a.Patch < b.Patch;
    }

    public static bool operator >(SemanticVersion a, SemanticVersion b)
    {
        return a.Major > b.Major || a.Major == b.Major && a.Minor > b.Minor || a.Major == b.Major && a.Minor == b.Minor && a.Patch > b.Patch;
    }

    public static bool operator ==(SemanticVersion a, SemanticVersion b)
    {
        return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch;
    }

    public static bool operator !=(SemanticVersion a, SemanticVersion b)
    {
        return a.Major != b.Major || a.Minor != b.Minor || a.Patch != b.Patch;
    }

    public static bool operator <=(SemanticVersion a, SemanticVersion b)
    {
        return a < b || a == b;
    }

    public static bool operator >=(SemanticVersion a, SemanticVersion b)
    {
        return a > b || a == b;
    }

    public override bool Equals(object obj)
    {
        return obj is SemanticVersion version && this == version;
    }

    public override int GetHashCode()
    {
        return Major.GetHashCode() ^ Minor.GetHashCode() ^ Patch.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }

    public SemanticVersion(string version)
    {
        version = version.Replace("v", "");

        var builder = "";
        // TODO do I ignore characters?
        foreach (var ch in version.ToCharArray())
        {
            if (char.IsDigit(ch) || ch == '.' || ch == '_')
            {
                builder += ch;
            }
        }
        version = builder;

        var versionSplit = version.Contains('.') ? version.Split('.') : version.Split('_');
        List<int> versionSplitValues = new();

        // force check all values
        foreach (var versionSplitValue in versionSplit)
        {
            if (!int.TryParse(versionSplitValue, out var versionValue))
            {
                // TODO dont throw exception
                throw new Exception($"Semantic version not a valid version");
            }

            versionSplitValues.Add(versionValue);
        }

        // remove anything after 3 version values
        if (versionSplitValues.Count > 3)
        {
            versionSplitValues.RemoveRange(3, versionSplitValues.Count - 3);
        }
        else if (versionSplitValues.Count < 3)
        {
            // add zeros to make version value count 3
            versionSplitValues.AddRange(Enumerable.Repeat(0, 3 - versionSplitValues.Count));
        }

        Major = versionSplitValues[0];
        Minor = versionSplitValues[1];
        Patch = versionSplitValues[2];
    }
}