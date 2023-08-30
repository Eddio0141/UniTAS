using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton(RegisterPriority.FirstUpdateSkipOnRestart)]
[ExcludeRegisterIfTesting]
public class FirstUpdateSkipOnRestart : IOnGameRestartResume, IOnInputUpdateActual, IOnLastUpdateActual,
    IOnPreUpdatesUnconditional, IOnInputUpdateUnconditional
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

    public FirstUpdateSkipOnRestart(IMonoBehaviourController monoBehaviourController, ILogger logger)
    {
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;
        _logger.LogDebug("Skipping first update after restart");
        _pendingState = PendingState.PendingPause;
    }

    public void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (fixedUpdate) return;

        if (_pendingState != PendingState.PendingPause) return;

        _pendingState = PendingState.PendingResumeLastUpdate;
        _logger.LogDebug("Pausing mono behaviour to skip an update");
        _monoBehaviourController.PausedUpdate = true;
    }

    public void InputUpdateUnconditional(bool fixedUpdate, bool newInputSystemUpdate)
    {
        ProcessResumeFinal("input update");
    }

    public void PreUpdateUnconditional()
    {
        ProcessResumeFinal("pre update");
    }

    private void ProcessResumeFinal(string updatePoint)
    {
        if (_pendingState != PendingState.PendingResumeFinal) return;

        _logger.LogDebug($"Skipped an update after restart at {updatePoint}, resuming mono behaviour");

        _pendingState = PendingState.PendingRestart;
        _monoBehaviourController.PausedUpdate = false;
    }

    public void OnLastUpdateActual()
    {
        if (_pendingState != PendingState.PendingResumeLastUpdate) return;
        _pendingState = PendingState.PendingResumeFinal;
    }
}