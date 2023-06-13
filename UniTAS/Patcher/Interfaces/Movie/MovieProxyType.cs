using System;
using MoonSharp.Interpreter.Interop;

namespace UniTAS.Patcher.Interfaces.Movie;

public abstract class MovieProxyType : IProxyFactory
{
    public abstract object CreateProxyObject(object o);
    public abstract Type TargetType { get; }
    public abstract Type ProxyType { get; }
}

public abstract class MovieProxyType<TProxy, TTarget> : MovieProxyType
    where TProxy : class
    where TTarget : class
{
    public override Type TargetType => typeof(TTarget);
    public override Type ProxyType => typeof(TProxy);

    public override object CreateProxyObject(object o)
    {
        return CreateProxyObject(o as TTarget);
    }

    protected abstract TProxy CreateProxyObject(TTarget target);
}