using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ContainerBindings.GameExecutionControllers;

public static class MonoBehaviourController
{
    private static IMonoBehaviourController _monoBehaviourController;

    static MonoBehaviourController()
    {
        ContainerStarter.RegisterContainerInitCallback(RegisterTiming.Entry,
            kernel => _monoBehaviourController = kernel.GetInstance<IMonoBehaviourController>());
    }

    public static bool PausedExecution
    {
        get => _monoBehaviourController.PausedExecution;
        set => _monoBehaviourController.PausedExecution = value;
    }

    public static bool PausedUpdate
    {
        get => _monoBehaviourController.PausedUpdate;
        set => _monoBehaviourController.PausedUpdate = value;
    }
}