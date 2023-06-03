using System;

namespace UniTAS.Patcher.Services;

public interface IGameRestart
{
    void SoftRestart(DateTime time);
    event GameRestartResume OnGameRestartResume;
    event GameRestart OnGameRestart;
}

public delegate void GameRestartResume(DateTime startupTime, bool preMonoBehaviourResume);

public delegate void GameRestart(DateTime startupTime, bool preSceneLoad);