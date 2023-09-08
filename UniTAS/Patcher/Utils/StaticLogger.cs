using System.Diagnostics;
using BepInEx.Logging;

namespace UniTAS.Patcher.Utils;

public static class StaticLogger
{
    public static ManualLogSource Log { get; } = Logger.CreateLogSource("UniTAS");

#if TRACE
    private static readonly ManualLogSource TraceLog = Logger.CreateLogSource("UniTAS-Trace");
#endif

    [Conditional("TRACE")]
    public static void Trace(object data)
    {
#if TRACE
        TraceLog.LogDebug(data);
#endif
    }
}