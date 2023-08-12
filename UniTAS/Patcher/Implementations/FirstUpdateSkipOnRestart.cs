using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton(RegisterPriority.FirstUpdateSkipOnRestart)]
[ExcludeRegisterIfTesting]
public class FirstUpdateSkipOnRestart : IOnGameRestartResume, IOnUpdateUnconditional, IOnInputUpdateUnconditional,
    IOnPreUpdatesUnconditional, IOnLastUpdateUnconditional
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

    public void InputUpdateUnconditional(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (_pendingState == PendingState.PendingResumeFinal)
        {
            _logger.LogDebug("Resuming mono behaviour, input update");
        }

        ProcessResumeFinal();
        if (fixedUpdate) return;
        ProcessUpdates();
    }

    public void UpdateUnconditional()
    {
        ProcessUpdates();
    }

    private void ProcessUpdates()
    {
        if (_pendingState == PendingState.PendingPause)
        {
            _pendingState = PendingState.PendingResumeLastUpdate;
            _logger.LogDebug("Pausing mono behaviour to skip an update");
            _monoBehaviourController.PausedUpdate = true;
        }
    }

    public void PreUpdateUnconditional()
    {
        if (_pendingState == PendingState.PendingResumeFinal)
        {
            _logger.LogDebug("Resuming mono behaviour, pre update");
        }

        ProcessResumeFinal();
    }

    private void ProcessResumeFinal()
    {
        if (_pendingState != PendingState.PendingResumeFinal) return;

        _pendingState = PendingState.PendingRestart;
        _logger.LogDebug("Resuming mono behaviour");
        _monoBehaviourController.PausedUpdate = false;
    }

    public void OnLastUpdateUnconditional()
    {
        if (_pendingState == PendingState.PendingResumeLastUpdate)
        {
            _pendingState = PendingState.PendingResumeFinal;
            _logger.LogDebug($"new pending state: {_pendingState}");
        }
    }
}