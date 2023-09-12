using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    public static IContainer Kernel { get; }

    static ContainerStarter()
    {
        StaticLogger.Log.LogDebug("Initializing container instance");

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

        var timings = Enum.GetValues(typeof(RegisterTiming)).Cast<RegisterTiming>();
        foreach (var timing in timings)
        {
            ContainerInitCallbacks.Add(timing, new());
        }

        StaticLogger.Log.LogDebug("Initialized container instance");
    }

    [InvokeOnUnityInit]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void UnityInit()
    {
        Init(RegisterTiming.UnityInit);
    }

    public static void Init(RegisterTiming timing)
    {
        StaticLogger.Log.LogDebug($"Registering types at timing {timing}");

        try
        {
            Kernel.Configure(c => Kernel.GetInstance<IDiscoverAndRegister>().Register<Logger>(c, timing));

            var forceInstantiateTypes = Kernel.GetInstance<IForceInstantiateTypes>();
            forceInstantiateTypes.InstantiateTypes<ForceInstantiateTypes>(timing);
        }
        catch (Exception e)
        {
            StaticLogger.Log.LogFatal(
                $"An exception occurred while initializing the container at timing {timing}\n{e}");
            throw;
        }

        StaticLogger.Log.LogDebug($"Registered types at timing {timing}");

        if (!ContainerInitCallbacks.TryGetValue(timing, out var callbacks)) return;
        ContainerInitCallbacks.Remove(timing);

        foreach (var callback in callbacks)
        {
            callback(Kernel);
        }
    }

    /// <summary>
    /// Adds a callback to be invoked when the container is initialized at the specified timing
    /// </summary>
    /// <param name="timing"></param>
    /// <param name="callback"></param>
    public static void RegisterContainerInitCallback(RegisterTiming timing, Action<IContainer> callback)
    {
        if (!ContainerInitCallbacks.TryGetValue(timing, out var callbacks))
        {
            callback(Kernel);
            return;
        }

        callbacks.Add(callback);
    }

    private static readonly Dictionary<RegisterTiming, List<Action<IContainer>>> ContainerInitCallbacks = new();
}