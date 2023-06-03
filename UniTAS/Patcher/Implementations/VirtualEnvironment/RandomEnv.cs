using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton]
public class RandomEnv : IRandomEnv, IOnGameRestart
{
    public long StartUpSeed { get; set; }
    public Random SystemRandom { get; private set; }

    private readonly IRandomWrapper _randomWrapper;

    public RandomEnv(IRandomWrapper randomWrapper)
    {
        _randomWrapper = randomWrapper;
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        SystemRandom = new((int)StartUpSeed);
        _randomWrapper.Seed = (int)StartUpSeed;
    }
}