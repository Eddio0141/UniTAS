using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
[ForceInstantiate]
public class FirstUpdateSkipOnRestart
{
    private enum PendingState
    {
        PendingRestart,
        PendingPause,
        PendingResumeLastUpdate,
    }

    private PendingState _pendingState = PendingState.PendingRestart;

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;

    private readonly IUpdateEvents _updateEvents;

    public FirstUpdateSkipOnRestart(IMonoBehaviourController monoBehaviourController, ILogger logger,
        IUpdateEvents updateEvents, IGameRestart gameRestart)
    {
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _updateEvents = updateEvents;
        gameRestart.OnGameRestartResume += OnGameRestartResume;
    }

    private void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;
        _pendingState = PendingState.PendingPause;

        _updateEvents.AddPriorityCallback(CallbackInputUpdate.InputUpdateActual, InputUpdateActual,
            CallbackPriority.FirstUpdateSkipOnRestart);
    }

    private void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (fixedUpdate || _pendingState != PendingState.PendingPause) return;

        _updateEvents.OnInputUpdateActual -= InputUpdateActual;

        _pendingState = PendingState.PendingResumeLastUpdate;
        _logger.LogDebug("Pausing MonoBehaviour to skip an update");
        _monoBehaviourController.PausedExecution = true;

        _updateEvents.AddPriorityCallback(CallbackUpdate.LastUpdateUnconditional, LastUpdate,
            CallbackPriority.FirstUpdateSkipOnRestartLastUpdate);
    }

    private void LastUpdate()
    {
        if (_pendingState != PendingState.PendingResumeLastUpdate) return;
        _pendingState = PendingState.PendingRestart;
        _updateEvents.OnLastUpdateUnconditional -= LastUpdate;
        _logger.LogDebug("Resuming MonoBehaviour after skipped an update");
        _monoBehaviourController.PausedExecution = false;
    }
}