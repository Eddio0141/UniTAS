using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI;

[Singleton]
public class GUIComponentFactory(IContainer container) : IGUIComponentFactory
{
    public T CreateComponent<T>() where T : IGUIComponent
    {
        return container.GetInstance<T>();
    }
}