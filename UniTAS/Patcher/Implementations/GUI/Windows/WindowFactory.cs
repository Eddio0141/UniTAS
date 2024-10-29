using System.Diagnostics.CodeAnalysis;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Register]
public class WindowFactory(IContainer container) : IWindowFactory
{
    public T Create<T>() where T : Window
    {
        return container.GetInstance<T>();
    }

    public void Create(UnityObjectIdentifier identifier)
    {
        container.With(identifier).GetInstance<ObjectTrackerInstanceWindow>();
    }

    public ObjectSearchConfigWindow Create(UnityObjectIdentifier.SearchSettings searchSettings)
    {
        return container.With(searchSettings).GetInstance<ObjectSearchConfigWindow>();
    }
}