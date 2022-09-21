using System;
using System.Reflection;

namespace UniTASPlugin.Patches;

public static class AuxilaryHelper
{
    public static Exception Cleanup_NeedsToBePatched(MethodBase original, Exception ex)
    {
        if (ex != null)
            Plugin.Log.LogError($"Failed to patch {original}: this method needs to be patched but something went wrong, exception: {ex}");
        return null;
    }

    public static Exception Cleanup_IgnoreException(MethodBase original, Exception ex)
    {
        if (ex != null)
            Plugin.Log.LogDebug($"Failed to patch: {original}, exception: {ex.Message}");
        return null;
    }
}