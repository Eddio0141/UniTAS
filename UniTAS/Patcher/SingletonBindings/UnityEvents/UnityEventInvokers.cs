using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.SingletonBindings.UnityEvents;

public static class UnityEventInvokers
{
    private static readonly IMonoBehEventInvoker MonoBehEventInvoker =
        ContainerStarter.Kernel.GetInstance<IMonoBehEventInvoker>();

    public static void InvokeAwake() => MonoBehEventInvoker.InvokeAwake();
    public static void InvokeStart() => MonoBehEventInvoker.InvokeStart();
    public static void InvokeUpdate() => MonoBehEventInvoker.InvokeUpdate();
    public static void InvokeFixedUpdate() => MonoBehEventInvoker.InvokeFixedUpdate();
    public static void InvokeLateUpdate() => MonoBehEventInvoker.InvokeLateUpdate();
    public static void InvokeOnEnable() => MonoBehEventInvoker.InvokeOnEnable();
}