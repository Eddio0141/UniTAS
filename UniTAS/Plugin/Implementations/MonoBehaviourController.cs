using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class MonoBehaviourController : IMonoBehaviourController
{
    public bool PausedExecution
    {
        get => Patcher.Shared.MonoBehaviourController.PausedExecution;
        set => Patcher.Shared.MonoBehaviourController.PausedExecution = value;
    }
}