using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using Object = UnityEngine.Object;

namespace UniTAS.Plugin.Implementations.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class GameRestart : IGameRestart, IOnAwakeUnconditional, IOnEnableUnconditional, IOnStartUnconditional,
    IOnUpdateUnconditional
{
    private DateTime _softRestartTime;

    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;
    private readonly ISceneWrapper _sceneWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;

    private readonly IOnGameRestart[] _onGameRestart;
    private readonly IOnGameRestartResume[] _onGameRestartResume;
    private readonly IOnPreGameRestart[] _onPreGameRestart;

    private readonly IStaticFieldManipulator _staticFieldManipulator;

    private bool _pendingRestart;
    private bool _pendingResumePausedExecution;
    private int _pendingSoftRestartCounter = -1;

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public GameRestart(ISyncFixedUpdateCycle syncFixedUpdate, ISceneWrapper sceneWrapper,
        IMonoBehaviourController monoBehaviourController, ILogger logger, IOnGameRestart[] onGameRestart,
        IOnGameRestartResume[] onGameRestartResume, IOnPreGameRestart[] onPreGameRestart,
        IStaticFieldManipulator staticFieldManipulator)
    {
        _syncFixedUpdate = syncFixedUpdate;
        _sceneWrapper = sceneWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _onGameRestart = onGameRestart;
        _onGameRestartResume = onGameRestartResume;
        _onPreGameRestart = onPreGameRestart;
        _staticFieldManipulator = staticFieldManipulator;
    }

    /// <summary>
    /// Destroys all necessary game objects to reset the game state.
    /// Default behaviour is to destroy all DontDestroyOnLoad objects.
    /// </summary>
    private void DestroyGameObjects()
    {
        var objs = Tracker.DontDestroyOnLoadRootObjects;
        _logger.LogDebug($"Destroying {objs.Count} DontDestroyOnLoad objects");

        foreach (var obj in objs)
        {
            var gameObject = (Object)obj;
            _logger.LogDebug($"Destroying {gameObject.name}");
            Object.Destroy(gameObject);
        }
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// </summary>
    /// <param name="time">Time to start the game at</param>
    public void SoftRestart(DateTime time)
    {
        if (_pendingRestart && !_pendingResumePausedExecution) return;

        OnPreGameRestart();

        _pendingRestart = true;
        _softRestartTime = time;

        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;

        DestroyGameObjects();

        _staticFieldManipulator.ResetStaticFields();

        // this invokes 2 frames before the sync since the counter is at 1
        _syncFixedUpdate.OnSync(() => _pendingSoftRestartCounter = 1);
        _logger.LogDebug("Soft restarting, pending FixedUpdate sync");
    }

    private void OnGameRestart(bool preSceneLoad)
    {
        foreach (var gameRestart in _onGameRestart)
        {
            gameRestart.OnGameRestart(_softRestartTime, preSceneLoad);
        }
    }

    protected virtual void OnGameRestartResume(bool preMonoBehaviourResume)
    {
        foreach (var gameRestart in _onGameRestartResume)
        {
            gameRestart.OnGameRestartResume(_softRestartTime, preMonoBehaviourResume);
        }
    }

    private void OnPreGameRestart()
    {
        foreach (var gameRestart in _onPreGameRestart)
        {
            gameRestart.OnPreGameRestart();
        }
    }

    private void SoftRestartOperation()
    {
        _logger.LogInfo("Soft restarting");

        OnGameRestart(true);
        _sceneWrapper.LoadScene(0);
        OnGameRestart(false);

        _pendingRestart = false;
        _pendingResumePausedExecution = true;
    }

    public void AwakeUnconditional()
    {
        PendingResumePausedExecution();
    }

    public void OnEnableUnconditional()
    {
        PendingResumePausedExecution();
    }

    public void StartUnconditional()
    {
        PendingResumePausedExecution();
    }

    public void UpdateUnconditional()
    {
        if (_pendingSoftRestartCounter > 0)
        {
            _pendingSoftRestartCounter--;
            return;
        }

        if (_pendingSoftRestartCounter == 0)
        {
            _pendingSoftRestartCounter--;
            SoftRestartOperation();
        }
        else
        {
            PendingResumePausedExecution();
        }
    }

    private void PendingResumePausedExecution()
    {
        if (!_pendingResumePausedExecution) return;
        OnGameRestartResume(true);

        _logger.LogInfo("Finish soft restarting");
        var actualTime = DateTime.Now;
        _logger.LogInfo($"System time: {actualTime}");

        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
        OnGameRestartResume(false);

        _pendingResumePausedExecution = false;
    }
}