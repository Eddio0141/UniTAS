namespace UniTAS.Plugin.Services;

public interface IGameSpeedUnlocker
{
    bool Unlock { get; }
    int OriginalTargetFrameRate { get; set; }
    int OriginalVSyncCount { get; set; }
}