using System;

namespace UniTAS.Plugin.Services;

public interface IGameRestart
{
    void SoftRestart(DateTime time);
    event GameRestartResume OnGameRestartResume;
}

public delegate void GameRestartResume(DateTime startupTime, bool preMonoBehaviourResume);