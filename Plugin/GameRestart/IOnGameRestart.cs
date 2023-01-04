using System;

namespace UniTASPlugin.GameRestart;

public interface IOnGameRestart
{
    void OnGameRestart(DateTime startupTime);
}