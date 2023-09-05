using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.Logging;

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
        PendingResumeFinal
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
        _logger.LogDebug("Skipping first update after restart");
        _pendingState = PendingState.PendingPause;

        _updateEvents.AddPriorityCallback(CallbackInputUpdate.InputUpdateUnconditional, InputUpdateActual,
            CallbackPriority.FirstUpdateSkipOnRestart);
    }

    private void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (_monoBehaviourController.PausedExecution || (!fixedUpdate && _monoBehaviourController.PausedUpdate))
            return;

        if (fixedUpdate) return;

        if (_pendingState != PendingState.PendingPause) return;

        _updateEvents.OnInputUpdateUnconditional -= InputUpdateActual;

        _pendingState = PendingState.PendingResumeLastUpdate;
        _logger.LogDebug("Pausing mono behaviour to skip an update");
        _monoBehaviourController.PausedUpdate = true;

        _updateEvents.AddPriorityCallback(CallbackUpdate.LastUpdateUnconditional, OnLastUpdateUnconditional,
            CallbackPriority.FirstUpdateSkipOnRestart);
    }

    private void InputUpdateUnconditional(bool fixedUpdate, bool newInputSystemUpdate)
    {
        ProcessResumeFinal("input update");
    }

    private void PreUpdateUnconditional()
    {
        ProcessResumeFinal("pre update");
    }

    private void ProcessResumeFinal(string updatePoint)
    {
        if (_pendingState != PendingState.PendingResumeFinal) return;
        _updateEvents.OnInputUpdateUnconditional -= InputUpdateUnconditional;
        _updateEvents.OnPreUpdatesUnconditional -= PreUpdateUnconditional;

        _logger.LogDebug($"Skipped an update after restart at {updatePoint}, resuming mono behaviour");

        _pendingState = PendingState.PendingRestart;
        _monoBehaviourController.PausedUpdate = false;
    }

    private void OnLastUpdateUnconditional()
    {
        if (_pendingState != PendingState.PendingResumeLastUpdate) return;
        _updateEvents.OnLastUpdateUnconditional -= OnLastUpdateUnconditional;

        _pendingState = PendingState.PendingResumeFinal;

        _updateEvents.AddPriorityCallback(CallbackInputUpdate.InputUpdateUnconditional, InputUpdateUnconditional,
            CallbackPriority.FirstUpdateSkipOnRestart);
        _updateEvents.AddPriorityCallback(CallbackUpdate.PreUpdateUnconditional, PreUpdateUnconditional,
            CallbackPriority.FirstUpdateSkipOnRestart);
    }
}