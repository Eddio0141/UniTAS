using System;
using System.Reflection;

namespace UniTASPlugin.Patches;

public static class Auxilary
{
    public static Exception Cleanup_IgnoreNotFound(MethodBase original, Exception ex)
    {
        var msgBuilder = "";
        if (original != null)
            msgBuilder += $"{original.DeclaringType}.{original.Name}";
        if (ex != null)
        {
            if (msgBuilder != "")
                msgBuilder += ": ";
            msgBuilder += ex;
        }

        Plugin.Log.LogDebug($"Failed to patch: {msgBuilder}");
        return null;
    }
}
