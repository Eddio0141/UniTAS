using System;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.Trackers.DontDestroyOnLoadTracker;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using Object = UnityEngine.Object;

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
    private readonly IStaticFieldManipulator _staticFieldManipulator;
    private readonly IDontDestroyOnLoadInfo _dontDestroyOnLoadInfo;

    public bool PendingRestart { get; private set; }
    private bool _pendingResumePausedExecution;

    public GameRestart(VirtualEnvironment virtualEnvironment, ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper, IMonoBehaviourController monoBehaviourController, ILogger logger,
        IOnGameRestart[] onGameRestart, IStaticFieldManipulator staticFieldManipulator,
        IDontDestroyOnLoadInfo dontDestroyOnLoadInfo, IOnGameRestartResume[] onGameRestartResume)
    {
        _virtualEnvironment = virtualEnvironment;
        _syncFixedUpdate = syncFixedUpdate;
        _unityWrapper = unityWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _onGameRestart = onGameRestart;
        _staticFieldManipulator = staticFieldManipulator;
        _dontDestroyOnLoadInfo = dontDestroyOnLoadInfo;
        _onGameRestartResume = onGameRestartResume;
    }

    private void DestroyDontDestroyOnLoads()
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
        PendingRestart = true;
        _softRestartTime = time;
        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;
        DestroyDontDestroyOnLoads();
        _staticFieldManipulator.ResetStaticFields();
        _syncFixedUpdate.OnSync(SoftRestartOperation, 1);
        _logger.LogDebug("Soft restarting, pending FixedUpdate call");
    }

    private void OnGameRestart()
    {
        foreach (var gameRestart in _onGameRestart)
        {
            gameRestart.OnGameRestart(_softRestartTime);
        }
    }

    private void OnGameRestartResume()
    {
        foreach (var gameRestart in _onGameRestartResume)
        {
            gameRestart.OnGameRestartResume(_softRestartTime);
        }
    }

    private void SoftRestartOperation()
    {
        _logger.LogInfo("Soft restarting");

        OnGameRestart();
        _unityWrapper.Scene.LoadScene(0);

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

    // plugin will call this as a backup
    public void FixedUpdate()
    {
        PendingResumePausedExecution();
    }

    private void PendingResumePausedExecution()
    {
        if (!_pendingResumePausedExecution) return;
        _pendingResumePausedExecution = false;
        OnGameRestartResume();

        _logger.LogDebug("random setting state");

        _unityWrapper.Random.Seed = (int)_virtualEnvironment.Seed;

        _logger.LogInfo("Finish soft restarting");
        var actualTime = DateTime.Now;
        _logger.LogInfo($"System time: {actualTime}");

        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
    }

    // TODO move setting random state to a separate class
}