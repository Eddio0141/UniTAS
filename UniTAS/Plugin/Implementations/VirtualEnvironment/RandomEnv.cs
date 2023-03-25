using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment;

[Singleton]
public class RandomEnv : IRandomEnv, IOnGameRestartResume
{
    public long StartUpSeed { get; set; }
    public Random SystemRandom { get; private set; }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        SystemRandom = new((int)StartUpSeed);
    }
}