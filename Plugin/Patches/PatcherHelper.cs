using System;
using System.Reflection;
using Diagnostics = System.Diagnostics;

namespace UniTASPlugin.Patches;

public static class PatcherHelper
{
    public static Exception Cleanup_IgnoreException(MethodBase original, Exception ex)
    {
        if (ex != null)
            Plugin.Instance.Log.LogDebug($"Failed to patch: {original}, exception: {ex}");
        return null;
    }

    public static bool CallFromPlugin()
    {
        var trace = new Diagnostics.StackTrace();
        var traceFrames = trace.GetFrames();
        if (traceFrames == null) return false;
        foreach (var frame in traceFrames)
        {
            var declaringType = frame.GetMethod().DeclaringType;
            var typeName = declaringType?.FullName;
            if (typeName == null) continue;
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