using BepInEx.Logging;

namespace Core;

public static class Logger
{
    public static ManualLogSource Log;

    static Logger()
    {
        Log = new ManualLogSource($"{PluginInfo.NAME}Core");
        BepInEx.Logging.Logger.Sources.Add(Log);
    }
}
