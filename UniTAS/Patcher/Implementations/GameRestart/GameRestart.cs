﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.TrackInfo;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using Object = UnityEngine.Object;

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
    private readonly IFinalizeSuppressor _finalizeSuppressor;
    private readonly IUpdateInvokeOffset _updateInvokeOffset;
    private readonly IObjectTracker _objectTracker;
    private readonly ICoroutine _coroutine;
    private readonly IGameInfo _gameInfo;

    private readonly IStaticFieldManipulator _staticFieldManipulator;
    private readonly ITimeEnv _timeEnv;

    private bool _pendingRestart;
    private bool _pendingResumePausedExecution;

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public GameRestart(ISyncFixedUpdateCycle syncFixedUpdate, ISceneManagerWrapper iSceneManagerWrapper,
        IMonoBehaviourController monoBehaviourController, ILogger logger, IOnGameRestart[] onGameRestart,
        IOnGameRestartResume[] onGameRestartResume, IOnPreGameRestart[] onPreGameRestart,
        IStaticFieldManipulator staticFieldManipulator, ITimeEnv timeEnv, IFinalizeSuppressor finalizeSuppressor,
        IUpdateInvokeOffset updateInvokeOffset, IObjectTracker objectTracker, ICoroutine coroutine, IGameInfo gameInfo,
        IInputEventInvoker inputEventInvoker, IUpdateEvents updateEvents)
    {
        _syncFixedUpdate = syncFixedUpdate;
        _iSceneManagerWrapper = iSceneManagerWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _staticFieldManipulator = staticFieldManipulator;
        _timeEnv = timeEnv;
        _finalizeSuppressor = finalizeSuppressor;
        _updateInvokeOffset = updateInvokeOffset;
        _objectTracker = objectTracker;
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

        // this has to be here, don't move to UnityEvents
        OnGameRestart += (_, preSceneLoad) =>
        {
            if (!preSceneLoad) return;
            inputEventInvoker.InputSystemEventsInit();
        };

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
    /// Destroys all necessary game objects to reset the game state.
    /// Default behaviour is to destroy all DontDestroyOnLoad objects.
    /// </summary>
    private void DestroyGameObjects()
    {
        var objs = _objectTracker.DontDestroyOnLoadRootObjects.ToList();
        _logger.LogDebug($"Destroying {objs.Count} DontDestroyOnLoad objects");

        foreach (var obj in objs)
        {
            _logger.LogDebug($"Destroying {obj.name}");
            Object.Destroy(obj);
        }
    }

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

        DestroyGameObjects();

        _logger.LogDebug("Disabling finalize invoke");
        // TODO is this even a good idea
        _finalizeSuppressor.DisableFinalizeInvoke = true;

        _staticFieldManipulator.ResetStaticFields();

        SerializationCallbackTracker.InvokeAllAfterDeserialization();

        _logger.LogDebug("Enabling finalize invoke");
        _finalizeSuppressor.DisableFinalizeInvoke = false;
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