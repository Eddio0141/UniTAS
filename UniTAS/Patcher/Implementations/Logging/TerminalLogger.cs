using BepInEx.Logging;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.Logging;

[Singleton]
public class TerminalLogger : ITerminalLogger
{
    private readonly ManualLogSource _logSource = new("UniTAS terminal");

    public TerminalLogger()
    {
        BepInEx.Logging.Logger.Sources.Add(_logSource);
    }

    public void LogMessage(object data)
    {
        _logSource.LogMessage(data);
    }
}