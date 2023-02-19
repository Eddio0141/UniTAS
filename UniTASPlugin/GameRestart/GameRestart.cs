using System;
using System.Diagnostics;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart.EventInterfaces;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.Trackers.DontDestroyOnLoadTracker;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameRestart : IGameRestart, IOnAwake, IOnEnable, IOnStart, IOnFixedUpdate
{
    private DateTime _softRestartTime;

    private readonly VirtualEnvironment _virtualEnvironment;
    private readonly ISyncFixedUpdate _syncFixedUpdate;
    private readonly IUnityWrapper _unityWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;

    private readonly IOnGameRestart[] _onGameRestart;
    private readonly IOnGameRestartResume[] _onGameRestartResume;
    private readonly IOnPreGameRestart[] _onPreGameRestart;

    private readonly IStaticFieldManipulator _staticFieldManipulator;
    private readonly IDontDestroyOnLoadInfo _dontDestroyOnLoadInfo;

    public bool PendingRestart { get; private set; }
    private bool _pendingResumePausedExecution;

    protected GameRestart(RestartParameters restartParameters)
    {
        _virtualEnvironment = restartParameters.VirtualEnvironment;
        _syncFixedUpdate = restartParameters.SyncFixedUpdate;
        _unityWrapper = restartParameters.UnityWrapper;
        _monoBehaviourController = restartParameters.MonoBehaviourController;
        _logger = restartParameters.Logger;
        _onGameRestart = restartParameters.OnGameRestart;
        _onGameRestartResume = restartParameters.OnGameRestartResume;
        _staticFieldManipulator = restartParameters.StaticFieldManipulator;
        _dontDestroyOnLoadInfo = restartParameters.DontDestroyOnLoadInfo;
        _onPreGameRestart = restartParameters.OnPreGameRestart;
    }

    /// <summary>
    /// Destroys all necessary game objects to reset the game state.
    /// Default behaviour is to destroy all DontDestroyOnLoad objects.
    /// </summary>
    protected virtual void DestroyGameObjects()
    {
        var dontDestroyOnLoads = _dontDestroyOnLoadInfo.DontDestroyOnLoadObjects;
        foreach (var obj in dontDestroyOnLoads)
        {
            _logger.LogDebug($"Removing DontDestroyOnLoad object {obj.GetType().FullName}, hash: {obj.GetHashCode()}");
            // Object.DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// </summary>
    /// <param name="time">Time to start the game at</param>
    public void SoftRestart(DateTime time)
    {
        if (PendingRestart && !_pendingResumePausedExecution) return;

        OnPreGameRestart();

        PendingRestart = true;
        _softRestartTime = time;

        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;

        DestroyGameObjects();

        _staticFieldManipulator.ResetStaticFields();

        _syncFixedUpdate.OnSync(SoftRestartOperation, 1);
        _logger.LogDebug("Soft restarting, pending FixedUpdate call");
    }

    private void OnGameRestart(bool preSceneLoad)
    {
        foreach (var gameRestart in _onGameRestart)
        {
            gameRestart.OnGameRestart(_softRestartTime, preSceneLoad);
        }
    }

    private void OnGameRestartResume(bool preMonoBehaviourResume)
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
        _unityWrapper.Scene.LoadScene(0);
        OnGameRestart(false);

        PendingRestart = false;
        _pendingResumePausedExecution = true;
    }

    public void Awake()
    {
        PendingResumePausedExecution();
    }

    public void OnEnable()
    {
        PendingResumePausedExecution();
    }

    public void Start()
    {
        PendingResumePausedExecution();
    }

    public void FixedUpdate()
    {
        PendingResumePausedExecution();
    }

    private void PendingResumePausedExecution()
    {
        if (!_pendingResumePausedExecution) return;
        OnGameRestartResume(true);

        _logger.LogDebug("random setting state");

        _unityWrapper.Random.Seed = (int)_virtualEnvironment.Seed;

        _logger.LogInfo("Finish soft restarting");
        var actualTime = DateTime.Now;
        _logger.LogInfo($"System time: {actualTime}");

        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
        OnGameRestartResume(false);

        _pendingResumePausedExecution = false;
    }

    // TODO move setting random state to a separate class
}