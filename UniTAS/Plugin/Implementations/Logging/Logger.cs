using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations.Logging;

public class Logger : ILogger
{
    public void LogFatal(object data)
    {
        Plugin.Log.LogFatal(data);
    }

    public void LogError(object data)
    {
        Plugin.Log.LogError(data);
    }

    public void LogWarning(object data)
    {
        Plugin.Log.LogWarning(data);
    }

    public void LogMessage(object data)
    {
        Plugin.Log.LogMessage(data);
    }

    public void LogInfo(object data)
    {
        Plugin.Log.LogInfo(data);
    }

    public void LogDebug(object data)
    {
        Plugin.Log.LogDebug(data);
    }
}