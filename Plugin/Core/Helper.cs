using System;
using System.Collections.Generic;
using System.Linq;

namespace Core;

internal static class Helper
{
    public static (int, int, int) SemanticVersioningFromString(string version)
    {
        version = version.Replace("v", "");
        version = version.Replace("_", "");

        var versionSplit = version.Split('.');
        var versionSplitValues = new List<int>();

        // force check all values
        foreach (var versionSplitValue in versionSplit)
        {
            if (!int.TryParse(versionSplitValue, out var versionValue))
            {
                throw new Exception("Semantic version not a valid version");
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

        return (versionSplitValues[0], versionSplitValues[1], versionSplitValues[2]);
    }
}