using System;

namespace UniTAS.Patcher.Services.VirtualEnvironment;

public interface IRandomEnv
{
    long StartUpSeed { get; set; }
    Random SystemRandom { get; }
}