using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(timing: RegisterTiming.Entry)]
public class MonoBehaviourController : IMonoBehaviourController
{
    public bool PausedExecution { get; set; }
}