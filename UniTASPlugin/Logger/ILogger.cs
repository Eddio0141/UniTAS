namespace UniTASPlugin.Logger;

public interface ILogger
{
    void LogFatal(object data);
    void LogError(object data);
    void LogWarning(object data);
    void LogMessage(object data);
    void LogInfo(object data);
    void LogDebug(object data);
}