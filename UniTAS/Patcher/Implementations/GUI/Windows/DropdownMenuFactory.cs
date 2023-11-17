using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
public class DropdownMenuFactory : IDropdownMenuFactory
{
    private readonly IContainer _container;

    private Window _currentDropdown;

    public DropdownMenuFactory(IContainer container)
    {
        _container = container;
    }

    public T Create<T>(DropdownEntry[] entries) where T : Window
    {
        _currentDropdown?.Close();
        _currentDropdown = _container.With("entries").EqualTo(entries).GetInstance<T>();
        return (T)_currentDropdown;
    }
}