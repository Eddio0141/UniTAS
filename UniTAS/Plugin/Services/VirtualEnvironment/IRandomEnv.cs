using System;

namespace UniTAS.Plugin.Services.VirtualEnvironment;

public interface IRandomEnv
{
    long StartUpSeed { get; set; }
    Random SystemRandom { get; }
}