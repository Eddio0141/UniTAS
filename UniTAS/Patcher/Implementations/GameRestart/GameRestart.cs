using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.TrackInfo;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using Object = UnityEngine.Object;
#if TRACE
using System.Diagnostics;
using UniTAS.Patcher.Utils;
#endif

namespace UniTAS.Patcher.Implementations.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
// target priority to after sync fixed update
[Singleton(RegisterPriority.GameRestart)]
public class GameRestart : IGameRestart, IOnAwakeUnconditional, IOnEnableUnconditional, IOnStartUnconditional,
    IOnFixedUpdateUnconditional
{
    private DateTime _softRestartTime;

    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;
    private readonly ISceneWrapper _sceneWrapper;
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
    public GameRestart(ISyncFixedUpdateCycle syncFixedUpdate, ISceneWrapper sceneWrapper,
        IMonoBehaviourController monoBehaviourController, ILogger logger, IOnGameRestart[] onGameRestart,
        IOnGameRestartResume[] onGameRestartResume, IOnPreGameRestart[] onPreGameRestart,
        IStaticFieldManipulator staticFieldManipulator, ITimeEnv timeEnv, IFinalizeSuppressor finalizeSuppressor,
        IUpdateInvokeOffset updateInvokeOffset, IObjectTracker objectTracker, ICoroutine coroutine, IGameInfo gameInfo)
    {
        _syncFixedUpdate = syncFixedUpdate;
        _sceneWrapper = sceneWrapper;
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
    }

    public bool Restarting => _pendingRestart || _pendingResumePausedExecution;

    /// <summary>
    /// Deinitializes GOG Galaxy so that it can be reinitialized again for games that use it
    /// </summary>
    private void DeinitGOGGalaxy()
    {
        var galaxyManager = AccessTools.TypeByName("Galaxy.Api.GalaxyInstance");
        if (galaxyManager != null)
        {
            _logger.LogDebug("Deinitializing GOG Galaxy");
            galaxyManager.GetMethod("Shutdown").Invoke(null, [true]);
        }
    }

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

        _coroutine.Start(SoftRestartCoroutine(time));
    }

    private IEnumerable<CoroutineWait> SoftRestartCoroutine(DateTime time)
    {
        while (!_gameInfo.IsFocused)
        {
            yield return new WaitForUpdateUnconditional();
        }

        _logger.LogInfo("Starting soft restart");

#if TRACE
        var sw = new Stopwatch();
        sw.Start();
#endif
        InvokeOnPreGameRestart();

        _pendingRestart = true;
        _softRestartTime = time;

        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;

        DestroyGameObjects();
        DeinitGOGGalaxy();

        _logger.LogDebug("Disabling finalize invoke");
        // TODO is this even a good idea
        _finalizeSuppressor.DisableFinalizeInvoke = true;

        _staticFieldManipulator.ResetStaticFields();

        _logger.LogDebug("Enabling finalize invoke");
        _finalizeSuppressor.DisableFinalizeInvoke = false;

#if TRACE
        sw.Stop();
        StaticLogger.Trace($"time elapsed for soft restart first phase: {sw.Elapsed.Milliseconds}ms");
#endif

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

#if TRACE
        var sw = new Stopwatch();
        sw.Start();
#endif

        OnGameRestart?.Invoke(_softRestartTime, true);
        _sceneWrapper.LoadScene(0);
        OnGameRestart?.Invoke(_softRestartTime, false);

#if TRACE
        sw.Stop();
        StaticLogger.Trace($"time elapsed for soft restart scene reload: {sw.Elapsed.Milliseconds}ms");
#endif

        _pendingRestart = false;
        _pendingResumePausedExecution = true;
    }

    public void AwakeUnconditional()
    {
        PendingResumePausedExecution("Awake");
    }

    public void OnEnableUnconditional()
    {
        PendingResumePausedExecution("OnEnable");
    }

    public void StartUnconditional()
    {
        PendingResumePausedExecution("Start");
    }

    public void FixedUpdateUnconditional()
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