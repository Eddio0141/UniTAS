using System;
using System.Reflection;

namespace UniTASPlugin.Patches;

public static class PatcherHelper
{
    public static Exception Cleanup_IgnoreException(MethodBase original, Exception ex)
    {
        if (ex != null)
            Plugin.Log.LogDebug($"Failed to patch: {original}, exception: {ex}");
        return null;
    }

    public static bool CallFromPlugin()
    {
        var trace = new System.Diagnostics.StackTrace();
        var traceFrames = trace.GetFrames();
        foreach (var frame in traceFrames)
        {
            var typeName = frame.GetMethod().DeclaringType.FullName;
            if (
                typeName.StartsWith("UniTASPlugin.ReversePatches") ||
                typeName.StartsWith("UniTASPlugin.Helper") ||
                typeName.StartsWith("BepInEx.Logging"))
            {
                return true;
            }
        }
        return false;
    }
}