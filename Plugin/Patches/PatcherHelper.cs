using System;
using System.Reflection;

namespace UniTASPlugin.Patches;

public static class PatcherHelper
{
    public static Exception Cleanup_IgnoreException(MethodBase original, Exception ex)
    {
        if (ex != null)
            Plugin.Instance.Logger.LogDebug($"Failed to patch: {original}, exception: {ex}");
        return null;
    }
}