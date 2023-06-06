using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Services.Invoker;
using UniTAS.Patcher.Services.NewInputSystem;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

public class InitInputSystemOverride
{
    [InvokeOnNewInputSystemInit]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void Init()
    {
        ContainerStarter.Kernel.GetInstance<IInputSystemOverride>();
    }
}