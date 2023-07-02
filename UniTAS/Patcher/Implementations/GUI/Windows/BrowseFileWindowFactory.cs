using StructureMap;
using StructureMap.Pipeline;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;

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
        var diArgs = new ExplicitArguments();
        diArgs.Set(args);

        return _container.GetInstance<IBrowseFileWindow>(diArgs);
    }
}