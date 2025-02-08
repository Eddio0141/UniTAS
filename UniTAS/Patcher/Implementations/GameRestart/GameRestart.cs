using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
// target priority to after sync fixed update
[Singleton]
public class GameRestart : IGameRestart
{
    private DateTime _softRestartTime;

    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;
    private readonly ISceneManagerWrapper _iSceneManagerWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;
    private readonly IUpdateInvokeOffset _updateInvokeOffset;
    private readonly ICoroutine _coroutine;
    private readonly IGameInfo _gameInfo;

    private readonly ITimeEnv _timeEnv;

    private bool _pendingRestart;
    private bool _pendingResumePausedExecution;

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public GameRestart(ISyncFixedUpdateCycle syncFixedUpdate, ISceneManagerWrapper iSceneManagerWrapper,
        IMonoBehaviourController monoBehaviourController, ILogger logger, IOnGameRestart[] onGameRestart,
        IOnGameRestartResume[] onGameRestartResume, IOnPreGameRestart[] onPreGameRestart, ITimeEnv timeEnv,
        IUpdateInvokeOffset updateInvokeOffset, ICoroutine coroutine, IGameInfo gameInfo, IUpdateEvents updateEvents)
    {
        _syncFixedUpdate = syncFixedUpdate;
        _iSceneManagerWrapper = iSceneManagerWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _timeEnv = timeEnv;
        _updateInvokeOffset = updateInvokeOffset;
        _coroutine = coroutine;
        _gameInfo = gameInfo;

        foreach (var gameRestartResume in onGameRestartResume)
        {
            OnGameRestartResume += gameRestartResume.OnGameRestartResume;
        }

        foreach (var gameRestart in onGameRestart)
        {
            OnGameRestart += gameRestart.OnGameRestart;
        }

        foreach (var gameRestart in onPreGameRestart)
        {
            OnPreGameRestart += gameRestart.OnPreGameRestart;
        }

        updateEvents.AddPriorityCallback(CallbackUpdate.AwakeUnconditional, AwakeUnconditional,
            CallbackPriority.GameRestart);
        updateEvents.AddPriorityCallback(CallbackUpdate.EnableUnconditional, OnEnableUnconditional,
            CallbackPriority.GameRestart);
        updateEvents.AddPriorityCallback(CallbackUpdate.StartUnconditional, StartUnconditional,
            CallbackPriority.GameRestart);
        updateEvents.AddPriorityCallback(CallbackUpdate.FixedUpdateUnconditional, FixedUpdateUnconditional,
            CallbackPriority.GameRestart);
    }

    public bool Restarting => _pendingRestart || _pendingResumePausedExecution;

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// </summary>
    /// <param name="time">Time to start the game at</param>
    public void SoftRestart(DateTime time)
    {
        if (_pendingRestart && !_pendingResumePausedExecution) return;

        _coroutine.Start(SoftRestartCoroutine(time)).OnComplete += status =>
        {
            if (status.Exception != null)
                _logger.LogFatal($"failed to soft restart: {status.Exception}");
        };
    }

    private IEnumerable<CoroutineWait> SoftRestartCoroutine(DateTime time)
    {
        if (!GameInfoManual.NoGraphics)
        {
            while (!_gameInfo.IsFocused)
            {
                yield return new WaitForUpdateUnconditional();
            }
        }

        _logger.LogInfo("Starting soft restart");

        var bench = Bench.Measure();
        InvokeOnPreGameRestart();

        _pendingRestart = true;
        _softRestartTime = time;

        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;

        bench.Dispose();

        _syncFixedUpdate.OnSync(SoftRestartOperation, -_timeEnv.FrameTime);
    }

    public event GameRestartResume OnGameRestartResume;
    public event Services.GameRestart OnGameRestart;
    public event Action OnPreGameRestart;

    protected virtual void InvokeOnGameRestartResume(bool preMonoBehaviourResume)
    {
        OnGameRestartResume?.Invoke(_softRestartTime, preMonoBehaviourResume);
    }

    private void InvokeOnPreGameRestart()
    {
        OnPreGameRestart?.Invoke();
    }

    private void SoftRestartOperation()
    {
        _logger.LogInfo("Soft restarting");

        OnGameRestart?.Invoke(_softRestartTime, true);
        _iSceneManagerWrapper.LoadScene(0);
        OnGameRestart?.Invoke(_softRestartTime, false);

        _pendingRestart = false;
        _pendingResumePausedExecution = true;
    }

    private void AwakeUnconditional()
    {
        PendingResumePausedExecution("Awake");
    }

    private void OnEnableUnconditional()
    {
        PendingResumePausedExecution("OnEnable");
    }

    private void StartUnconditional()
    {
        PendingResumePausedExecution("Start");
    }

    private void FixedUpdateUnconditional()
    {
        PendingResumePausedExecution("FixedUpdate");
    }

    private void PendingResumePausedExecution(string timing)
    {
        if (!_pendingResumePausedExecution) return;
        _pendingResumePausedExecution = false;

        InvokeOnGameRestartResume(true);

        _logger.LogInfo("Finish soft restarting");
        var actualTime = DateTime.Now;
        _logger.LogInfo($"System time: {actualTime}");

        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug($"Resuming MonoBehaviour execution at {timing}, {_updateInvokeOffset.Offset}");
        InvokeOnGameRestartResume(false);
    }
}