using BepInEx.Configuration;
using StructureMap;
using UniTAS.Patcher.Implementations.DependencyInjection;
using UniTAS.Patcher.Implementations.Logging;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Utils;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container(c =>
        {
            c.ForSingletonOf<ConfigFile>().Use(_ => Plugin.PluginConfig);

            c.ForSingletonOf<PluginWrapper>().Use<PluginWrapper>();

            c.ForSingletonOf<DiscoverAndRegister>().Use<DiscoverAndRegister>();
            c.For<IDiscoverAndRegister>().Use(x => x.GetInstance<DiscoverAndRegister>());

            c.ForSingletonOf<Logger>().Use<Logger>();
            c.For<ILogger>().Use(x => x.GetInstance<Logger>());
        });

        container.Configure(c => container.GetInstance<IDiscoverAndRegister>().Register<PluginWrapper>(c));

        var forceInstantiateTypes = container.GetInstance<IForceInstantiateTypes>();
        forceInstantiateTypes.InstantiateTypes<Plugin>();

        return container;
    }
}