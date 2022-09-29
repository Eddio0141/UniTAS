using System;
using System.Reflection;

namespace UniTASPlugin.ReversePatches;

public static class AuxilaryHelper
{
    public static Exception Cleanup_IgnoreException(MethodBase original, Exception ex)
    {
        if (ex != null)
            Plugin.Log.LogDebug($"Failed to reverse patch: {original}, exception: {ex.Message}");
        return null;
    }
}
