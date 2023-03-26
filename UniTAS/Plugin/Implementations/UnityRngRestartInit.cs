using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Register]
public class UnityRngRestartInit : IOnGameRestartResume
{
    private readonly IRandomWrapper _random;
    private readonly IRandomEnv _randomEnv;

    public UnityRngRestartInit(IRandomEnv randomEnv, IRandomWrapper random)
    {
        _randomEnv = randomEnv;
        _random = random;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;

        _random.Seed = (int)_randomEnv.StartUpSeed;
    }
}