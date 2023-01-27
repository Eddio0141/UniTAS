using System;
using System.Linq;
using BepInEx;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameInitialRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameInitialRestart : IGameInitialRestart, IOnAwake, IOnEnable, IOnStart, IOnFixedUpdate
{
    private readonly IUnityWrapper _unityWrapper;
    private readonly ILogger _logger;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly IStaticFieldManipulator _staticFieldManipulator;
    private readonly ISyncFixedUpdate _syncFixedUpdate;

    private bool _pendingResumePausedExecution;

    private bool _restartOperationStarted;

    public GameInitialRestart(IUnityWrapper unityWrapper, ILogger logger,
        IMonoBehaviourController monoBehaviourController, IStaticFieldManipulator staticFieldManipulator,
        ISyncFixedUpdate syncFixedUpdate)
    {
        _unityWrapper = unityWrapper;
        _logger = logger;
        _monoBehaviourController = monoBehaviourController;
        _staticFieldManipulator = staticFieldManipulator;
        _syncFixedUpdate = syncFixedUpdate;
    }

    public void InitialRestart()
    {
        if (_restartOperationStarted) return;
        _restartOperationStarted = true;
        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;
        DestroyAllGameObjects();

        _staticFieldManipulator.ResetStaticFields();
        _syncFixedUpdate.OnSync(SoftRestartOperation, 1);
        _logger.LogDebug("Soft restarting, pending FixedUpdate call");
    }

    private void SoftRestartOperation()
    {
        _logger.LogInfo("Restarting");
        _unityWrapper.SceneWrapper.LoadScene(0);
        _pendingResumePausedExecution = true;
    }

    private void DestroyAllGameObjects()
    {
        var allObjects = _unityWrapper.Object.FindObjectsOfType(_unityWrapper.Object.ObjectType).ToArray();
        _logger.LogDebug($"Attempting destruction of {allObjects.Length} objects");
        foreach (var obj in allObjects)
        {
            if (obj is BaseUnityPlugin)
            {
                _logger.LogDebug($"Found BepInEx type: {obj.GetType().FullName}, skipping");
                continue;
            }

            try
            {
                _unityWrapper.MonoBehaviour.StopAllCoroutines(obj);
                _unityWrapper.Object.DestroyImmediate(obj);
            }
            catch (Exception)
            {
                // ignored
            }
        }
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