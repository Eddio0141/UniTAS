using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class KernelDebugInfo
{
    public KernelDebugInfo(ILogger logger)
    {
        logger.LogDebug($"Register info\n{ContainerStarter.Kernel.WhatDoIHave()}");
    }
}