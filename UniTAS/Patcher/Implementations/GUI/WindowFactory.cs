using System.Diagnostics.CodeAnalysis;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Register]
public class WindowFactory : IWindowFactory
{
    private readonly IContainer _container;

    public WindowFactory(IContainer container)
    {
        _container = container;
    }

    public T Create<T>() where T : Window
    {
        return _container.GetInstance<T>();
    }
}