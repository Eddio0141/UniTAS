using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Implementations.GameRestart;

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

    private readonly IOnPreGameRestart[] _onPreGameRestart;

    private readonly IStaticFieldManipulator _staticFieldManipulator;
    private readonly ITimeEnv _timeEnv;

    private bool _pendingRestart;
    private bool _pendingResumePausedExecution;
    private int _pendingSoftRestartCounter = -1;

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public GameRestart(ISyncFixedUpdateCycle syncFixedUpdate, ISceneWrapper sceneWrapper,
        IMonoBehaviourController monoBehaviourController, ILogger logger, IOnGameRestart[] onGameRestart,
        IOnGameRestartResume[] onGameRestartResume, IOnPreGameRestart[] onPreGameRestart,
        IStaticFieldManipulator staticFieldManipulator, ITimeEnv timeEnv)
    {
        _syncFixedUpdate = syncFixedUpdate;
        _sceneWrapper = sceneWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _onPreGameRestart = onPreGameRestart;
        _staticFieldManipulator = staticFieldManipulator;
        _timeEnv = timeEnv;

        foreach (var gameRestartResume in onGameRestartResume)
        {
            OnGameRestartResume += gameRestartResume.OnGameRestartResume;
        }

        foreach (var gameRestart in onGameRestart)
        {
            OnGameRestart += gameRestart.OnGameRestart;
        }
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

        _logger.LogInfo("Starting soft restart");

        OnPreGameRestart();

        _pendingRestart = true;
        _softRestartTime = time;

        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;

        DestroyGameObjects();

        _staticFieldManipulator.ResetStaticFields();

        // this invokes 2 frames before the sync since the counter is at 1
        _syncFixedUpdate.OnSync(() => _pendingSoftRestartCounter = 1, -_timeEnv.FrameTime * 1.0);
        _logger.LogDebug("Soft restarting, pending FixedUpdate sync");
    }

    public event GameRestartResume OnGameRestartResume;
    public event Services.GameRestart OnGameRestart;

    protected virtual void InvokeOnGameRestartResume(bool preMonoBehaviourResume)
    {
        OnGameRestartResume?.Invoke(_softRestartTime, preMonoBehaviourResume);
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

        OnGameRestart?.Invoke(_softRestartTime, true);
        _sceneWrapper.LoadScene(0);
        OnGameRestart?.Invoke(_softRestartTime, false);

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
        InvokeOnGameRestartResume(true);

        _logger.LogInfo("Finish soft restarting");
        var actualTime = DateTime.Now;
        _logger.LogInfo($"System time: {actualTime}");

        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
        InvokeOnGameRestartResume(false);

        _pendingResumePausedExecution = false;
    }
}