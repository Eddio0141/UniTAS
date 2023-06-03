using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class MonoBehaviourController : IMonoBehaviourController
{
    public bool PausedExecution
    {
        get => StaticServices.MonoBehaviourController.PausedExecution;
        set => StaticServices.MonoBehaviourController.PausedExecution = value;
    }

    public bool PausedUpdate
    {
        get => StaticServices.MonoBehaviourController.PausedUpdate;
        set => StaticServices.MonoBehaviourController.PausedUpdate = value;
    }
}