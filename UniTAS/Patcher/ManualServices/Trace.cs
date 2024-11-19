using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
#if TRACE
using HarmonyLib;
using UniTAS.Patcher.Utils;
#endif

namespace UniTAS.Patcher.ManualServices;

public static class Trace
{
    [MustUseReturnValue]
    public static IDisposable MethodStart([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null,
        (string, object)[] methodArgs = null)
    {
#if TRACE
        StaticLogger.TraceLog.LogDebug(
            $"[{path}:{lineNumber}] ENTRY, {FormatArgs(methodArgs)}");

        return new Tracer(lineNumber, path, methodArgs);
#else
        return new NoOpDisposable();
#endif
    }

#if TRACE
    private class Tracer(int lineNumber, string path, (string, object)[] methodArgs) : IDisposable
    {
        public void Dispose()
        {
            StaticLogger.TraceLog.LogDebug(
                $"[{path}:{lineNumber}] EXIT, {FormatArgs(methodArgs)}");
        }
    }

    private static string FormatArgs((string, object)[] methodArgs)
    {
        return methodArgs?.Join(converter: pair => $"{pair.Item1}: {DebugHelp.PrintClass(pair.Item2)}");
    }
#else
    private class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
#endif
}