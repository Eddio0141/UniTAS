using System;

namespace UniTASPlugin.GameRestart;

public interface IOnGameRestartResume
{
    void OnGameRestartResume(DateTime startupTime);
}