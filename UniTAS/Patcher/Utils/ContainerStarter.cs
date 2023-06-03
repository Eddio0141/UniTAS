using StructureMap;
using UniTAS.Patcher.Implementations.DependencyInjection;
using UniTAS.Patcher.Implementations.Logging;
using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Utils;

public static class ContainerStarter
{
    public static IContainer Kernel { get; private set; }

    [InvokeOnPatcherFinish]
    public static void Init()
    {
        Kernel = new Container(c =>
        {
            c.ForSingletonOf<DiscoverAndRegister>().Use<DiscoverAndRegister>();
            c.For<IDiscoverAndRegister>().Use(x => x.GetInstance<DiscoverAndRegister>());

            c.ForSingletonOf<Logger>().Use<Logger>();
            c.For<ILogger>().Use(x => x.GetInstance<Logger>());
        });

        Kernel.Configure(c => Kernel.GetInstance<IDiscoverAndRegister>().Register<Logger>(c));

        var forceInstantiateTypes = Kernel.GetInstance<IForceInstantiateTypes>();
        forceInstantiateTypes.InstantiateTypes<Logger>();
    }
}