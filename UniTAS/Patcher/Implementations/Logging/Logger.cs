using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Logging;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Logger : ILogger
{
    public void LogFatal(object data)
    {
        StaticLogger.Log.LogFatal(data);
    }

    public void LogError(object data)
    {
        StaticLogger.Log.LogError(data);
    }

    public void LogWarning(object data)
    {
        StaticLogger.Log.LogWarning(data);
    }

    public void LogMessage(object data)
    {
        StaticLogger.Log.LogMessage(data);
    }

    public void LogInfo(object data)
    {
        StaticLogger.Log.LogInfo(data);
    }

    public void LogDebug(object data)
    {
        StaticLogger.Log.LogDebug(data);
    }
}