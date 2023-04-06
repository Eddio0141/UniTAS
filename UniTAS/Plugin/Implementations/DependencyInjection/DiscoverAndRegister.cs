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
        var types = AccessTools.GetTypesFromAssembly(typeof(TAssemblyContainingType).Assembly);

        var registers = new List<RegisterInfoBase>();

        foreach (var type in types)
        {
            registers.AddRange(GetRegisterInfos(type));
        }

        // order by priority
        registers = registers.OrderBy(x => x.Priority).ToList();

        foreach (var register in registers)
        {
            switch (register)
            {
                case RegisterInfo registerInfo:
                    if (registerInfo.IsSingleton)
                    {
                        config.For(registerInfo.Type).Singleton();
                    }

                    RegisterInterfaces(registerInfo.Type, config, registerInfo.IncludeDifferentAssembly,
                        registerInfo.RegisterAttribute);
                    break;
                case RegisterAllInfo registerAllInfo:
                    config.For(registerAllInfo.Type).Use(registerAllInfo.InnerType);
                    break;
            }
        }
    }

    private static void RegisterInterfaces(Type type, IRegistry config, bool includeDifferentAssembly,
        RegisterAttribute registerAttribute)
    {
        var interfaces = type.GetInterfaces();
        var baseType = type.BaseType;
        var typeAssembly = type.Assembly;

        if (baseType != null && baseType != typeof(object) &&
            registerAttribute?.IgnoreInterfaces?.All(x => x != baseType) is true or null &&
            (includeDifferentAssembly || Equals(baseType.Assembly, typeAssembly)))
        {
            config.For(baseType).Use(x => x.GetInstance(type));
        }

        foreach (var @interface in interfaces)
        {
            if (!(includeDifferentAssembly || Equals(@interface.Assembly, typeAssembly)))
                continue;

            config.For(@interface).Use(x => x.GetInstance(type));
        }
    }

    private IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type)
    {
        var dependencyInjectionAttributes = type.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);
        if (dependencyInjectionAttributes.Length == 0)
            yield break;

        if (dependencyInjectionAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute) && _isTesting)
            yield break;

        foreach (var dependencyInjectionAttribute in dependencyInjectionAttributes)
        {
            switch (dependencyInjectionAttribute)
            {
                case SingletonAttribute singletonAttribute:
                    yield return new RegisterInfo(type, singletonAttribute.IncludeDifferentAssembly,
                        singletonAttribute,
                        true);
                    break;
                case RegisterAllAttribute registerAllAttribute:
                {
                    var allTypes = AccessTools.GetTypesFromAssembly(type.Assembly);
                    var types = type.IsInterface
                        ? allTypes.Where(x => x.GetInterfaces().Contains(type))
                        : allTypes.Where(x => x.IsSubclassOf(type));

                    foreach (var innerType in types)
                    {
                        var innerTypeAttributes =
                            innerType.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);
                        var excludeTesting = innerTypeAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute) &&
                                             _isTesting;
                        if (excludeTesting) continue;

                        var registerAttribute = innerTypeAttributes.FirstOrDefault(x => x is RegisterAttribute);

                        yield return new RegisterAllInfo(type, innerType, registerAllAttribute);
                        var innerRegisterInfos = GetRegisterInfos(innerType);
                        foreach (var innerRegisterInfo in innerRegisterInfos)
                        {
                            yield return innerRegisterInfo;
                        }

                        yield return new RegisterInfo(innerType, false,
                            registerAttribute as RegisterAttribute ?? registerAllAttribute,
                            false);
                    }

                    break;
                }
                case RegisterAttribute registerAttribute:
                    yield return new RegisterInfo(type, registerAttribute.IncludeDifferentAssembly,
                        registerAttribute,
                        false);
                    break;
                case ExcludeRegisterIfTestingAttribute:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}