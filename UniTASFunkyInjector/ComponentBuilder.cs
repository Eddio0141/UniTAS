using System;

namespace UniTASFunkyInjector;

public class ComponentBuilder<TService>
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; private set; }
    public Lifetime Lifetime { get; private set; } = Lifetime.Transient;

    public ComponentBuilder()
    {
        ServiceType = typeof(TService);
    }

    public ComponentBuilder<TService> ImplementedBy<TImplementation>()
    {
        ImplementationType = typeof(TImplementation);
        return this;
    }

    public ComponentBuilder<TService> LifestyleSingleton()
    {
        Lifetime = Lifetime.Singleton;
        return this;
    }
}