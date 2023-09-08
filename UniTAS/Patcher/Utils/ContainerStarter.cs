using System;
using System.Diagnostics.CodeAnalysis;
using StructureMap;
using UniTAS.Patcher.Implementations.DependencyInjection;
using UniTAS.Patcher.Implementations.Logging;
using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Utils;

public static class ContainerStarter
{
    public static IContainer Kernel { get; private set; }

    [InvokeOnUnityInit]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void UnityInit()
    {
        Init(RegisterTiming.UnityInit);
    }

    public static void Init(RegisterTiming timing)
    {
        if (Kernel == null)
        {
            StaticLogger.Log.LogDebug("Initializing container");

            try
            {
                Kernel = new Container(c =>
                {
                    c.ForSingletonOf<DiscoverAndRegister>().Use<DiscoverAndRegister>();
                    c.For<IDiscoverAndRegister>().Use(x => x.GetInstance<DiscoverAndRegister>());

                    c.ForSingletonOf<Logger>().Use<Logger>();
                    c.For<ILogger>().Use(x => x.GetInstance<Logger>());
                });
            }
            catch (Exception e)
            {
                StaticLogger.Log.LogFatal($"An exception occurred while initializing the container\n{e}");
                throw;
            }
        }

        try
        {
            Kernel.Configure(c => Kernel.GetInstance<IDiscoverAndRegister>().Register<Logger>(c, timing));

            var forceInstantiateTypes = Kernel.GetInstance<IForceInstantiateTypes>();
            forceInstantiateTypes.InstantiateTypes<ForceInstantiateTypes>();
        }
        catch (Exception e)
        {
            StaticLogger.Log.LogFatal(
                $"An exception occurred while initializing the container at timing {timing}\n{e}");
            throw;
        }
    }
}