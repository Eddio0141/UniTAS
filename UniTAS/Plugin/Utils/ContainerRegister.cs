using BepInEx.Configuration;
using StructureMap;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Interfaces.GUI;
using UniTAS.Plugin.Interfaces.Patches.PatchProcessor;
using UniTAS.Plugin.Interfaces.TASRenderer;

namespace UniTAS.Plugin.Utils;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.AssemblyContainingType<Plugin>();
                scanner.Convention<DependencyInjectionConvention>();

                scanner.AddAllTypesOf<PatchProcessor>();
                scanner.AddAllTypesOf<IMainMenuTab>();
                scanner.AddAllTypesOf<VideoRenderer>();
                scanner.AddAllTypesOf<AudioRenderer>();
            });

            c.ForSingletonOf<ConfigFile>().Use(_ => Plugin.PluginConfig);

            c.ForSingletonOf<PluginWrapper>().Use<PluginWrapper>();
        });

        return container;
    }
}