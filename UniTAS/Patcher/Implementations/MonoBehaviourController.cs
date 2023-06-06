using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class MonoBehaviourController : IMonoBehaviourController
{
    public bool PausedExecution
    {
        get => Utils.MonoBehaviourController.PausedExecution;
        set => Utils.MonoBehaviourController.PausedExecution = value;
    }

    public bool PausedUpdate
    {
        get => Utils.MonoBehaviourController.PausedUpdate;
        set => Utils.MonoBehaviourController.PausedUpdate = value;
    }
}