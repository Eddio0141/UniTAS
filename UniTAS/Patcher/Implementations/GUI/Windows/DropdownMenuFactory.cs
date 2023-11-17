using System.Collections.Generic;
using StructureMap;
using StructureMap.Pipeline;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;
using UnityEngine;

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

    public T Create<T>(DropdownEntry[] entries, Vector2 position) where T : Window
    {
        _currentDropdown?.Close();
        var args = new ExplicitArguments(new Dictionary<string, object>
        {
            { "entries", entries },
            { "position", position }
        });
        _currentDropdown = _container.GetInstance<T>(args);
        return (T)_currentDropdown;
    }
}