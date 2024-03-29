using System;
using StructureMap;
using StructureMap.Configuration.DSL;
using UniTAS.Patcher.Interfaces.DependencyInjection;

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

        // register type itself
        config.For(type).Use(type);

        // register with base type
        if (baseType != null && baseType != typeof(object) &&
            // registerAttribute?.IgnoreInterfaces?.All(x => x != baseType) is true or null &&
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
}