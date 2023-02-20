using System;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart.EventInterfaces;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;

namespace UniTAS.Plugin.GameRestart.Events;

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