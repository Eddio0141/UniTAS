using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton]
public class BrowseFileWindowFactory : IBrowseFileWindowFactory
{
    private readonly IContainer _container;

    public BrowseFileWindowFactory(IContainer container)
    {
        _container = container;
    }

    public IBrowseFileWindow Open(BrowseFileWindowArgs args)
    {
        return _container.GetInstance<IBrowseFileWindow>(new ConstructorArg(nameof(args), args));
    }
}