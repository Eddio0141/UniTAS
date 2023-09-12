using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ContainerBindings.GameExecutionControllers;

public static class FinalizeSuppressor
{
    private static IFinalizeSuppressor _finalizeSuppressor;

    static FinalizeSuppressor()
    {
        ContainerStarter.RegisterContainerInitCallback(RegisterTiming.Entry,
            kernel => _finalizeSuppressor = kernel.GetInstance<IFinalizeSuppressor>());
    }

    public static bool DisableFinalizeInvoke
    {
        // this should be fine as I don't really use this until later in the UniTAS load phase
        get => _finalizeSuppressor?.DisableFinalizeInvoke ?? false;
        set => _finalizeSuppressor.DisableFinalizeInvoke = value;
    }
}