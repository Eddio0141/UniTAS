namespace UniTASPlugin.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UnityInstanceWrapFactory : IUnityInstanceWrapFactory
{
    public T Create<T>(object instance) where T : UnityInstanceWrap
    {
        return (T)System.Activator.CreateInstance(typeof(T), instance);
    }

    public T CreateNew<T>(params object[] args) where T : UnityInstanceWrap
    {
        // creates the wrapped instance with null, then calls the constructor with the args
        var instance = (T)System.Activator.CreateInstance(typeof(T), null);
        instance.NewInstance(args);
        return instance;
    }
}