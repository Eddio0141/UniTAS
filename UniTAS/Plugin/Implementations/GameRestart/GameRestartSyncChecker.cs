using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations.GameRestart;

[Singleton]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class GameRestartSyncChecker : IOnFixedUpdateUnconditional, IOnUpdateUnconditional, IOnGameRestartResume
{
    private bool _pendingCheck;
    private bool _fixedUpdateInvoked;

    private readonly ILogger _logger;

    public GameRestartSyncChecker(ILogger logger)
    {
        _logger = logger;
    }

    public void FixedUpdateUnconditional()
    {
        if (!_pendingCheck || _fixedUpdateInvoked) return;
        _fixedUpdateInvoked = true;
    }

    public void UpdateUnconditional()
    {
        if (!_pendingCheck) return;
        _pendingCheck = false;

        if (!_fixedUpdateInvoked)
        {
            _logger.LogError("FixedUpdate was not invoked after soft restart, this means physics update isn't in sync");
        }
    }


    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;

        _pendingCheck = true;
        _fixedUpdateInvoked = false;
    }
}