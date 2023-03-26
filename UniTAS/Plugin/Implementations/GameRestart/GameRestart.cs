using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using Object = UnityEngine.Object;

namespace UniTAS.Plugin.Implementations.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton()]
public class GameRestart : IGameRestart, IOnAwake, IOnEnable, IOnStart, IOnFixedUpdate
{
    private DateTime _softRestartTime;

    private readonly ISyncFixedUpdate _syncFixedUpdate;
    private readonly ISceneWrapper _sceneWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;

    private readonly IOnGameRestart[] _onGameRestart;
    private readonly IOnGameRestartResume[] _onGameRestartResume;
    private readonly IOnPreGameRestart[] _onPreGameRestart;

    private readonly IStaticFieldManipulator _staticFieldManipulator;

    public bool PendingRestart { get; private set; }
    private bool _pendingResumePausedExecution;

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public GameRestart(RestartParameters restartParameters)
    {
        _syncFixedUpdate = restartParameters.SyncFixedUpdate;
        _sceneWrapper = restartParameters.SceneWrapper;
        _monoBehaviourController = restartParameters.MonoBehaviourController;
        _logger = restartParameters.Logger;
        _onGameRestart = restartParameters.OnGameRestart;
        _onGameRestartResume = restartParameters.OnGameRestartResume;
        _staticFieldManipulator = restartParameters.StaticFieldManipulator;
        _onPreGameRestart = restartParameters.OnPreGameRestart;
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

        _logger.LogInfo("Finish soft restarting");
        var actualTime = DateTime.Now;
        _logger.LogInfo($"System time: {actualTime}");

        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
        OnGameRestartResume(false);

        _pendingResumePausedExecution = false;
    }
}