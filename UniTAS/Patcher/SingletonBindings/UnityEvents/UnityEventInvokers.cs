using StructureMap;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.SingletonBindings.UnityEvents;

public static class UnityEventInvokers
{
    private static IMonoBehEventInvoker _monoBehEventInvoker;

    public static void Init(IContainer kernel)
    {
        // for whatever reason I can't use ContainerStart.RegisterContainerInitCallback here
        _monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();
    }

    public static void InvokeAwake() => _monoBehEventInvoker?.InvokeAwake();

    public static void InvokeStart() => _monoBehEventInvoker?.InvokeStart();

    public static void InvokeUpdate() => _monoBehEventInvoker?.InvokeUpdate();

    public static void InvokeFixedUpdate() => _monoBehEventInvoker?.InvokeFixedUpdate();

    public static void InvokeLateUpdate() => _monoBehEventInvoker?.InvokeLateUpdate();

    public static void InvokeOnEnable() => _monoBehEventInvoker?.InvokeOnEnable();
}