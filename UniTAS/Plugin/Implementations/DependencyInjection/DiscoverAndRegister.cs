using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StructureMap;
using StructureMap.Configuration.DSL;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations.DependencyInjection;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class DiscoverAndRegister : IDiscoverAndRegister
{
    private readonly bool _isTesting = AccessTools.TypeByName("Xunit.FactAttribute") != null;

    public void Register<TAssemblyContainingType>(ConfigurationExpression config)
    {
        var allTypes = AccessTools.GetTypesFromAssembly(typeof(TAssemblyContainingType).Assembly);
        var types = allTypes.Where(x => x.GetCustomAttributes(typeof(DependencyInjectionAttribute), true).Length > 0);

        var registers = new List<RegisterInfoBase>();

        foreach (var type in types)
        {
            registers.AddRange(GetRegisterInfos(type, allTypes));
        }

        // order by priority
        registers = registers.OrderBy(x => x.Priority).ToList();

        foreach (var register in registers)
        {
            switch (register)
            {
                case RegisterInfo registerInfo:
                    if (registerInfo.RegisterAttribute is SingletonAttribute)
                    {
                        config.For(registerInfo.Type).Singleton();
                    }

                    RegisterInterfaces(registerInfo.Type, config, registerInfo.RegisterAttribute);
                    break;
                case RegisterAllInfo registerAllInfo:
                    config.For(registerAllInfo.Type).Use(registerAllInfo.InnerType);
                    break;
            }
        }
    }

    private static void RegisterInterfaces(Type type, IRegistry config, RegisterAttribute registerAttribute)
    {
        var interfaces = type.GetInterfaces();
        var baseType = type.BaseType;
        var typeAssembly = type.Assembly;

        // register with base type
        if (baseType != null && baseType != typeof(object) &&
            registerAttribute?.IgnoreInterfaces?.All(x => x != baseType) is true or null &&
            ((registerAttribute?.IncludeDifferentAssembly ?? false) || Equals(baseType.Assembly, typeAssembly)))
        {
            config.For(baseType).Use(x => x.GetInstance(type));
        }

        // register with all interfaces
        foreach (var @interface in interfaces)
        {
            if (!((registerAttribute?.IncludeDifferentAssembly ?? false) || Equals(@interface.Assembly, typeAssembly)))
                continue;

            config.For(@interface).Use(x => x.GetInstance(type));
        }
    }

    private IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes)
    {
        var dependencyInjectionAttributes = type.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);

        // early return if ExcludeRegisterIfTestingAttribute is present
        if (_isTesting && dependencyInjectionAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute)) yield break;

        foreach (var dependencyInjectionAttribute in dependencyInjectionAttributes)
        {
            switch (dependencyInjectionAttribute)
            {
                case SingletonAttribute singletonAttribute:
                    yield return new RegisterInfo(type, singletonAttribute);
                    break;
                case RegisterAllAttribute registerAllAttribute:
                {
                    var types = type.IsInterface
                        ? allTypes.Where(x => x.GetInterfaces().Contains(type))
                        : allTypes.Where(x => x.IsSubclassOf(type) && !x.IsAbstract);

                    // if type is abstract, recursively register inner types
                    foreach (var innerType in types)
                    {
                        var innerTypeAttributes =
                            innerType.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);
                        var excludeTesting = _isTesting &&
                                             innerTypeAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute);
                        if (excludeTesting) continue;

                        yield return new RegisterAllInfo(type, innerType, registerAllAttribute);

                        var innerRegisterInfos = GetRegisterInfos(innerType, allTypes);
                        foreach (var innerRegisterInfo in innerRegisterInfos)
                        {
                            yield return innerRegisterInfo;
                        }
                    }

                    break;
                }
                case RegisterAttribute registerAttribute:
                    yield return new RegisterInfo(type, registerAttribute);
                    break;
                case ExcludeRegisterIfTestingAttribute:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}