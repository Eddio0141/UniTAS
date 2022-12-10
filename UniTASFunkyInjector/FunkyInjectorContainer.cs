using System;
using System.Collections.Generic;
using System.Linq;

namespace UniTASFunkyInjector;

public class FunkyInjectorContainer
{
    private readonly Dictionary<Type, List<Component>> _registeredTypes = new();

    public void Register<TService>(ComponentBuilder<TService> builder)
    {
        if (!_registeredTypes.ContainsKey(typeof(TService)))
        {
            _registeredTypes.Add(typeof(TService), new());
        }

        _registeredTypes[typeof(TService)].Add(Component.Build(builder));
    }

    public TService Resolve<TService>()
    {
        return (TService)Resolve(typeof(TService));
    }

    private object Resolve(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "Type cannot be null");
        }

        if (type.GetConstructors().Length > 0)
        {
            return ResolveConstructor(type);
        }

        if (!_registeredTypes.ContainsKey(type))
        {
            throw new Exception($"Type {type.Name} is not registered");
        }

        var components = _registeredTypes[type];
        var component = components[0];

        var implType = component.ImplementationType;
        var instance = component.Lifetime == Lifetime.Singleton ? component.Instance : ResolveConstructor(implType);

        return instance;
    }

    private object ResolveConstructor(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "Type cannot be null");
        }

        Component singletonComponent = null;
        if (_registeredTypes.ContainsKey(type))
        {
            singletonComponent = _registeredTypes[type].First();

            if (singletonComponent.Lifetime == Lifetime.Singleton)
            {
                if (singletonComponent.Instance != null)
                {
                    return singletonComponent.Instance;
                }
            }
            else
            {
                singletonComponent = null;
            }
        }

        var constructor = type.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var args = new List<object>();
        foreach (var parameter in parameters)
        {
            // if its an IEnumerable, resolve all implementations
            if (parameter.ParameterType.IsGenericType &&
                parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                // use resolve all to get all implementations
                var implementations = ResolveAll(parameter.ParameterType.GetGenericArguments()[0]);

                // change the type
                var listType = typeof(List<>).MakeGenericType(parameter.ParameterType.GetGenericArguments()[0]);
                var list = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");
                foreach (var implementation in implementations)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    addMethod.Invoke(list, new[] { implementation });
                }

                args.Add(list);
            }
            else
            {
                args.Add(Resolve(parameter.ParameterType));
            }
        }

        var instance = constructor.Invoke(args.ToArray());

        if (singletonComponent != null)
        {
            singletonComponent.Instance = instance;
        }

        return instance;
    }

    public IEnumerable<T> ResolveAll<T>()
    {
        return ResolveAll(typeof(T)).Select(x => (T)x);
    }

    private IEnumerable<object> ResolveAll(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "Type cannot be null");
        }

        if (!_registeredTypes.ContainsKey(type))
        {
            throw new Exception($"Type {type.Name} is not registered");
        }

        return _registeredTypes[type].Select(x => Resolve(x.ImplementationType)).ToArray();
    }
}