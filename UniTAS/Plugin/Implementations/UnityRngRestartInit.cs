using System;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
public class UnityRngRestartInit : IOnGameRestartResume
{
    private readonly IRandomWrapper _random;
    private readonly VirtualEnvironment _virtualEnvironment;

    public UnityRngRestartInit(VirtualEnvironment virtualEnvironment, IRandomWrapper random)
    {
        _virtualEnvironment = virtualEnvironment;
        _random = random;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;

        _random.Seed = (int)_virtualEnvironment.Seed;
    }
}