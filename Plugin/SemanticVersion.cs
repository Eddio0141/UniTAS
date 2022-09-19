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
        return a.Major < b.Major || (a.Major == b.Major && a.Minor < b.Minor) || (a.Major == b.Major && a.Minor == b.Minor && a.Patch < b.Patch);
    }

    public static bool operator >(SemanticVersion a, SemanticVersion b)
    {
        return a.Major > b.Major || (a.Major == b.Major && a.Minor > b.Minor) || (a.Major == b.Major && a.Minor == b.Minor && a.Patch > b.Patch);
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
        if (obj is SemanticVersion version)
        {
            return this == version;
        }

        return false;
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

        string builder = "";
        // TODO do I ignore characters?
        foreach (char ch in version.ToCharArray())
        {
            if (char.IsDigit(ch) || ch == '.' || ch == '_')
            {
                builder += ch;
            }
        }
        version = builder;

        string[] versionSplit;
        if (version.Contains('.'))
        {
            versionSplit = version.Split('.');
        }
        else
        {
            versionSplit = version.Split('_');
        }
        List<int> versionSplitValues = new();

        // force check all values
        foreach (string versionSplitValue in versionSplit)
        {
            if (!int.TryParse(versionSplitValue, out int versionValue))
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