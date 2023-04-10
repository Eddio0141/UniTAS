using BepInEx.Configuration;
using StructureMap;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Services;

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
        });

        container.Configure(c => container.GetInstance<IDiscoverAndRegister>().Register<PluginWrapper>(c));

        return container;
    }
}