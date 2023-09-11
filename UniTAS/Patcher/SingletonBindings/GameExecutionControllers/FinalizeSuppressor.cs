using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.SingletonBindings.GameExecutionControllers;

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
        get => _finalizeSuppressor.DisableFinalizeInvoke;
        set => _finalizeSuppressor.DisableFinalizeInvoke = value;
    }
}