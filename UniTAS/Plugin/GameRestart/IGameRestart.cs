using System;

namespace UniTAS.Plugin.GameRestart;

public interface IGameRestart
{
    void SoftRestart(DateTime time);
    bool PendingRestart { get; }
}