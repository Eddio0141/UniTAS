using System;

namespace Core;

public static class Log
{
    static Action<object> logDebug;
    static Action<object> logError;
    static Action<object> logFatal;
    static Action<object> logInfo;
    static Action<object> logMessage;
    static Action<object> logWarning;

    public static void SetLoggers(Action<object> logDebug, Action<object> logError, Action<object> logFatal, Action<object> logInfo, Action<object> logMessage, Action<object> logWarning)
    {
        Log.logDebug = logDebug;
        Log.logError = logError;
        Log.logFatal = logFatal;
        Log.logInfo = logInfo;
        Log.logMessage = logMessage;
        Log.logWarning = logWarning;
    }

    public static void LogDebug(object message)
    {
        logDebug?.Invoke(message);
    }

    public static void LogError(object message)
    {
        logError?.Invoke(message);
    }

    public static void LogFatal(object message)
    {
        logFatal?.Invoke(message);
    }

    public static void LogInfo(object message)
    {
        logInfo?.Invoke(message);
    }

    public static void LogMessage(object message)
    {
        logMessage?.Invoke(message);
    }

    public static void LogWarning(object message)
    {
        logWarning?.Invoke(message);
    }
}
