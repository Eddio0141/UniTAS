using System;

namespace UniTAS.Plugin.UnitySafeWrappers;

public abstract class UnityInstanceWrap
{
    public object Instance { get; private set; }
    protected abstract Type WrappedType { get; }

    protected UnityInstanceWrap(object instance)
    {
        Instance = instance;
    }

    /// <summary>
    /// Creates a new instance of the wrapped type with the given args
    /// </summary>
    /// <param name="args">Arguments passed to the constructor, which tries to match the best constructor for this</param>
    public virtual void NewInstance(params object[] args)
    {
        Instance = Activator.CreateInstance(WrappedType, args);
    }
}