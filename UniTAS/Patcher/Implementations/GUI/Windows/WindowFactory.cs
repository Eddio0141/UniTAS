using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Register]
public class WindowFactory(IContainer container) : IWindowFactory
{
    public T Create<T>() where T : Window
    {
        return container.GetInstance<T>();
    }

    public ObjectTrackerInstanceWindow Create(UnityObjectIdentifier identifier)
    {
        return container.GetInstance<ObjectTrackerInstanceWindow>(new ConstructorArg(nameof(identifier), identifier));
    }

    public ObjectSearchConfigWindow Create(UnityObjectIdentifier.SearchSettings searchSettings)
    {
        return container.GetInstance<ObjectSearchConfigWindow>(new ConstructorArg(nameof(searchSettings),
            searchSettings));
    }

    public ObjectPickerSearchSettings Create(ObjectPickerWindow.SearchSettings searchSettings)
    {
        return container.GetInstance<ObjectPickerSearchSettings>(new ConstructorArg(nameof(searchSettings),
            searchSettings));
    }
}