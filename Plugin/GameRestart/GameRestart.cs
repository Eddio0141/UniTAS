using System;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameRestart : IGameRestart, IOnAwake, IOnEnable, IOnStart, IOnFixedUpdate
{
    private DateTime _softRestartTime;

    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;
    private readonly ISyncFixedUpdate _syncFixedUpdate;
    private readonly IUnityWrapper _unityWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;

    private readonly IOnGameRestart[] _onGameRestart;
    private readonly IStaticFieldManipulator _staticFieldManipulator;

    public bool PendingRestart { get; private set; }
    private bool _pendingResumePausedExecution;

    public GameRestart(IVirtualEnvironmentFactory virtualEnvironmentFactory, ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper, IMonoBehaviourController monoBehaviourController, ILogger logger,
        IOnGameRestart[] onGameRestart, IStaticFieldManipulator staticFieldManipulator)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
        _syncFixedUpdate = syncFixedUpdate;
        _unityWrapper = unityWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _onGameRestart = onGameRestart;
        _staticFieldManipulator = staticFieldManipulator;
    }

    private void DestroyDontDestroyOnLoads()
    {
        // TODO make this cleaner
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// </summary>
    /// <param name="time">Time to start the game at</param>
    public void SoftRestart(DateTime time)
    {
        PendingRestart = true;
        _softRestartTime = time;
        StopScriptExecution();
        DestroyDontDestroyOnLoads();
        _staticFieldManipulator.ResetStaticFields();
        OnGameRestart();
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

    private void SoftRestartOperation()
    {
        _logger.LogInfo("Soft restarting");

        _unityWrapper.SceneWrapper.LoadScene(0);

        _logger.LogDebug("random setting state");

        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        RandomWrap.InitState((int)env.Seed);

        _logger.LogInfo("Finish soft restarting");
        _logger.LogInfo($"System time: {DateTime.Now}");

        PendingRestart = false;
        _pendingResumePausedExecution = true;
    }

    private void StopScriptExecution()
    {
        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;
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
        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
    }
}