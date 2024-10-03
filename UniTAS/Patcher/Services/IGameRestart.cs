using System;

namespace UniTAS.Patcher.Services;

public interface IGameRestart
{
    void SoftRestart(DateTime time);
    event GameRestartResume OnGameRestartResume;
    event GameRestart OnGameRestart;
    event Action OnPreGameRestart;
    bool Restarting { get; }
}

public delegate void GameRestartResume(DateTime startupTime, bool preMonoBehaviourResume);

public delegate void GameRestart(DateTime startupTime, bool preSceneLoad);