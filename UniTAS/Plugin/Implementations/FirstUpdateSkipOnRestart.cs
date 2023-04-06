using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Models.DependencyInjection;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations;

[Singleton(RegisterPriority.FirstUpdateSkipOnRestart)]
public class FirstUpdateSkipOnRestart : IOnGameRestartResume, IOnUpdateUnconditional, IOnLastUpdateUnconditional
{
    private bool _pendingSkipUpdate;

    private readonly IMonoBehaviourController _monoBehaviourController;

    public FirstUpdateSkipOnRestart(IMonoBehaviourController monoBehaviourController)
    {
        _monoBehaviourController = monoBehaviourController;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;
        _pendingSkipUpdate = true;
    }

    public void UpdateUnconditional()
    {
        if (!_pendingSkipUpdate) return;
        _monoBehaviourController.PausedExecution = true;
    }

    public void OnLastUpdateUnconditional()
    {
        if (!_pendingSkipUpdate) return;
        _pendingSkipUpdate = false;
        _monoBehaviourController.PausedExecution = false;
    }
}