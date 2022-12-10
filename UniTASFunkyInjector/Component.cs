using System;

namespace UniTASFunkyInjector;

internal class Component
{
    public Type ImplementationType { get; }
    public Lifetime Lifetime { get; }
    public object Instance { get; set; }

    private Component(Type implementationType, Lifetime lifetime)
    {
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }

    public static Component Build<T>(ComponentBuilder<T> builder)
    {
        var component = new Component(builder.ImplementationType, builder.Lifetime);

        return component;
    }
}