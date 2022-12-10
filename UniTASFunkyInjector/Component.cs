using System;

namespace UniTASFunkyInjector;

internal class Component
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public Lifetime Lifetime { get; }
    public object Instance { get; set; }

    private Component(Type serviceType, Type implementationType, Lifetime lifetime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }

    public static Component Build<T>(ComponentBuilder<T> builder)
    {
        var component = new Component(builder.ServiceType, builder.ImplementationType, builder.Lifetime);

        return component;
    }
}