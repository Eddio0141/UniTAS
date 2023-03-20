using StructureMap;
using UniTAS.Plugin.Services.UnitySafeWrappers;

namespace UniTAS.Plugin.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UnityInstanceWrapFactory : IUnityInstanceWrapFactory
{
    private readonly IContainer _container;

    public UnityInstanceWrapFactory(IContainer container)
    {
        _container = container;
    }

    public T Create<T>(object instance) where T : UnityInstanceWrap
    {
        // because the wrap could have dependencies, we need to use the container to create it, and pass the instance to the constructor
        return _container.With(instance).GetInstance<T>();
    }

    public T CreateNew<T>(params object[] args) where T : UnityInstanceWrap
    {
        // creates the wrapped instance with null, then calls the constructor with the args
        var instance = _container.With(typeof(object[]), null).GetInstance<T>();
        instance.NewInstance(args);
        return instance;
    }
}