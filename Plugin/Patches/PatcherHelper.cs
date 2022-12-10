using System;
using System.Diagnostics;
using System.Linq;
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

    public static bool InvokedFromCriticalNamespace()
    {
        var rev = Plugin.Kernel.Resolve<PatchReverseInvoker>();
        var trace = rev.Invoke(() => new StackTrace());

        var criticalNamespaces = new[]
        {
            "BepInEx"
        };

        return trace.GetFrames()
            .Any(f => criticalNamespaces.Any(n => f.GetMethod().DeclaringType.FullName.StartsWith(n)));
    }
}