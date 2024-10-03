using System;

namespace UniTAS.Patcher.Utils;

public static class ConfigUtils
{
    public static string GetEntryKey(string configRaw, string entry, string key)
    {
        var entryStart = configRaw.IndexOf($"[{entry}]", StringComparison.InvariantCulture);
        if (entryStart == -1) return null;

        var keyStart = configRaw.IndexOf(key, entryStart, StringComparison.InvariantCulture);
        if (keyStart == -1) return null;

        var valueStart = configRaw.IndexOf("=", keyStart, StringComparison.InvariantCulture);
        if (valueStart == -1) return null;

        var valueEnd = configRaw.IndexOf("\n", valueStart, StringComparison.InvariantCulture);

        return configRaw.Substring(valueStart + 1, valueEnd - valueStart - 1).Trim();
    }
}