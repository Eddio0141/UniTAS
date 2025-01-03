using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ninject;
using Ninject.Parameters;
using UniTAS.Patcher.ContainerBindings.UnityEvents;
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

        IKernel kernel;
        try
        {
            kernel = new StandardKernel();
            kernel.Bind<IDiscoverAndRegister>().To<DiscoverAndRegister>().InSingletonScope();
            kernel.Bind<ILogger>().To<Logger>().InSingletonScope();
        }
        catch (Exception e)
        {
            StaticLogger.Log.LogFatal($"An exception occurred while initializing the container\n{e}");
            throw;
        }

        Kernel = new Container(kernel);
        kernel.Bind<IContainer>().To<Container>().InSingletonScope();

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
        UnityEventInvokers.Init(Kernel);
    }

    public static void Init(RegisterTiming timing)
    {
        StaticLogger.Log.LogDebug($"Registering types at timing {timing}");

        try
        {
            Kernel.GetInstance<IDiscoverAndRegister>().Register<Logger>(Kernel, timing);

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

public interface IContainer
{
    T GetInstance<T>(params ConstructorArg[] args);
    object GetInstance(Type type, params ConstructorArg[] args);
    IEnumerable<T> GetAllInstances<T>(params ConstructorArg[] args);
    IEnumerable<object> GetAllInstances(Type type);
    IKernel RawKernel { get; }
}

public class Container(IKernel rawKernel) : IContainer
{
    public T GetInstance<T>(params ConstructorArg[] args) => RawKernel.Get<T>(ToParams(args));

    public object GetInstance(Type type, params ConstructorArg[] args) => RawKernel.Get(type, ToParams(args));

    public IEnumerable<T> GetAllInstances<T>(params ConstructorArg[] args) => RawKernel.GetAll<T>(ToParams(args));
    public IEnumerable<object> GetAllInstances(Type type) => RawKernel.GetAll(type);

    public IKernel RawKernel { get; } = rawKernel;

    private static IParameter[] ToParams(ConstructorArg[] args) => args.Select(IParameter (a) => a.ArgRaw).ToArray();
}

public class ConstructorArg(string name, object value)
{
    public readonly ConstructorArgument ArgRaw = new(name, value);
}