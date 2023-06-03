using BepInEx.Logging;

namespace UniTAS.Patcher.Utils;

public static class StaticLogger
{
    public static ManualLogSource Log { get; } = new("UniTAS");
}