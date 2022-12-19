using System;

namespace UniTASPlugin.GameRestart;

public interface IGameRestart
{
    void SoftRestart(DateTime time);
    bool PendingRestart { get; }
}