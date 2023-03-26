using System;
using System.Linq;
using HarmonyLib;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using UniTAS.Plugin.Interfaces.DependencyInjection;

namespace UniTAS.Plugin.Implementations.DependencyInjection;

public class DependencyInjectionConvention : IRegistrationConvention
{
    private static readonly bool IsTesting = AccessTools.TypeByName("Xunit.FactAttribute") != null;

    public void Process(Type type, Registry registry)
    {
        RecursiveRegister(type, registry);
    }

    private static void RecursiveRegister(Type type, IRegistry registry)
    {
        var dependencyInjectionAttributes = type.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);
        if (dependencyInjectionAttributes.Length == 0)
            return;

        if (dependencyInjectionAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute) && IsTesting) return;

        foreach (var dependencyInjectionAttribute in dependencyInjectionAttributes)
        {
            switch (dependencyInjectionAttribute)
            {
                case SingletonAttribute singletonAttribute:
                    registry.For(type).Singleton();
                    RegisterInterfaces(type, registry, singletonAttribute.IncludeDifferentAssembly, singletonAttribute);
                    break;
                case RegisterAttribute registerAttribute:
                    RegisterInterfaces(type, registry, registerAttribute.IncludeDifferentAssembly, registerAttribute);
                    break;
                case RegisterAllAttribute:
                {
                    var types = AccessTools.GetTypesFromAssembly(type.Assembly).Where(x => x.IsSubclassOf(type));
                    foreach (var innerType in types)
                    {
                        var innerTypeAttributes =
                            innerType.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);
                        var excludeTesting = innerTypeAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute) &&
                                             IsTesting;
                        if (excludeTesting) continue;

                        var registerAttribute = innerTypeAttributes.FirstOrDefault(x => x is RegisterAttribute);

                        registry.For(type).Use(innerType);
                        RegisterInterfaces(innerType, registry, false, registerAttribute as RegisterAttribute);
                        RecursiveRegister(innerType, registry);
                    }

                    break;
                }
                case ExcludeRegisterIfTestingAttribute:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void RegisterInterfaces(Type type, IRegistry registry, bool includeDifferentAssembly,
        RegisterAttribute registerAttribute)
    {
        var interfaces = type.GetInterfaces();
        var baseType = type.BaseType;
        var typeAssembly = type.Assembly;

        if (baseType != null && baseType != typeof(object) &&
            registerAttribute?.IgnoreInterfaces?.All(x => x != baseType) is true or null &&
            (includeDifferentAssembly || Equals(baseType.Assembly, typeAssembly)))
        {
            registry.For(baseType).Use(x => x.GetInstance(type));
        }

        foreach (var @interface in interfaces)
        {
            if (!(includeDifferentAssembly || Equals(@interface.Assembly, typeAssembly)))
                continue;

            registry.For(@interface).Use(x => x.GetInstance(type));
        }
    }
}