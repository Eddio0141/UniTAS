using BepInEx.Configuration;
using StructureMap;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Implementations.Logging;
using UniTAS.Plugin.Services.DependencyInjection;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Utils;

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