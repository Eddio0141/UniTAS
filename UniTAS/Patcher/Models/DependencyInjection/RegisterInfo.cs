using System;
using StructureMap;
using StructureMap.Configuration.DSL;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Models.DependencyInjection;

public class RegisterInfo : RegisterInfoBase
{
    private RegisterAttribute RegisterAttribute { get; }

    public RegisterInfo(Type type, RegisterAttribute registerAttribute, RegisterTiming timing) : base(
        registerAttribute.Priority, timing)
    {
        Type = type;
        RegisterAttribute = registerAttribute;
    }

    public override void Register(ConfigurationExpression config)
    {
        if (RegisterAttribute is SingletonAttribute)
        {
            config.For(Type).Singleton();
        }

        RegisterInterfaces(Type, config, RegisterAttribute);
    }

    private static void RegisterInterfaces(Type type, IRegistry config, RegisterAttribute registerAttribute)
    {
        var interfaces = type.GetInterfaces();
        var baseType = type.BaseType;
        var typeAssembly = type.Assembly;
        var includeDiffAssembly = registerAttribute?.IncludeDifferentAssembly ?? false;

        // register type itself
        config.For(type).Use(type);

        StaticLogger.Trace($"Registering {type.SaneFullName()}");

        // register with base type
        if (baseType != null && baseType != typeof(object) &&
            // registerAttribute?.IgnoreInterfaces?.All(x => x != baseType) is true or null &&
            (includeDiffAssembly || Equals(baseType.Assembly, typeAssembly)))
        {
            StaticLogger.Trace($"Registering base type {baseType.SaneFullName()}");
            config.For(baseType).Use(x => x.GetInstance(type));
        }

        // register with all interfaces
        foreach (var @interface in interfaces)
        {
            if (!(includeDiffAssembly || Equals(@interface.Assembly, typeAssembly)))
                continue;

            StaticLogger.Trace($"Registering interface {@interface.SaneFullName()}");
            config.For(@interface).Use(x => x.GetInstance(type));
        }

        // now go through inheritance
        var inheritanceType = baseType?.BaseType;

        while (inheritanceType is not null && inheritanceType != typeof(object))
        {
            // base type
            if (includeDiffAssembly || Equals(inheritanceType.Assembly, typeAssembly))
            {
                config.For(inheritanceType).Use(x => x.GetInstance(type));
                StaticLogger.Trace($"Registering base type {inheritanceType.SaneFullName()}");
            }

            inheritanceType = inheritanceType.BaseType;
        }
    }
}