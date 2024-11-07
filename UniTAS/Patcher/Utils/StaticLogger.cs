#if TRACE
using System;
#endif
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

namespace UniTAS.Patcher.Utils;

public static class StaticLogger
{
    public static ManualLogSource Log { get; } = Logger.CreateLogSource("UniTAS");

#if TRACE
    private static readonly ManualLogSource TraceLog = Logger.CreateLogSource("UniTAS-Trace");
#endif

    [Conditional("TRACE")]
    // ReSharper disable once UnusedParameter.Global
    public static void Trace(object data, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
    {
#if TRACE
        // path would be like "some_path/UniTAS/UniTAS/Patcher/Utils/StaticLogger.cs"
        // removal from "some_path/UniTAS/UniTAS" to "Patcher/Utils/StaticLogger.cs"
        if (path != null)
        {
            var patcherIndex = path.IndexOf("Patcher", StringComparison.InvariantCulture);
            if (patcherIndex >= 0)
            {
                path = path.Substring(patcherIndex + "Patcher".Length + 1);
            }
        }

        TraceLog.LogDebug($"[{path}:{lineNumber}] {data}");
#endif
    }

    public static void LogDebug(object data) => Log.LogDebug(data);
    public static void LogError(object data) => Log.LogError(data);
    public static void LogWarning(object data) => Log.LogWarning(data);
}