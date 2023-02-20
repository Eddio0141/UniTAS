using UniTASPlugin.Logger;
using UniTASPlugin.StaticFieldStorage;

namespace UniTASPlugin.Tests.Kernel;

public class FakeLogger : ILogger
{
    public void LogFatal(object data)
    {
    }

    public void LogError(object data)
    {
    }

    public void LogWarning(object data)
    {
    }

    public void LogMessage(object data)
    {
    }

    public void LogInfo(object data)
    {
    }

    public void LogDebug(object data)
    {
    }
}

public class FakeStaticFieldStorage : IStaticFieldManipulator
{
    public FakeStaticFieldStorage(ILogger logger)
    {
    }

    public void ResetStaticFields()
    {
    }
}