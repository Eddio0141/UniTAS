using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;

namespace UniTAS.Patcher.Implementations;

[Singleton(timing: RegisterTiming.Entry)]
public class FinalizeSuppressor : IFinalizeSuppressor
{
    public bool DisableFinalizeInvoke { get; set; }
}