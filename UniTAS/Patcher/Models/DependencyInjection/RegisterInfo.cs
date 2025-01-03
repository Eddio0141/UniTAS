using System;
using System.Collections.Generic;
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

    public override void Register(IContainer container)
    {
        var interfaces = Type.GetInterfaces();
        var baseType = Type.BaseType;
        var typeAssembly = Type.Assembly;
        var includeDiffAssembly = RegisterAttribute?.IncludeDifferentAssembly ?? false;

        StaticLogger.Trace($"Registering {Type.SaneFullName()}");

        var bindTypes = new List<Type>();

        // register with base type
        if (baseType != null && baseType != typeof(object) &&
            // registerAttribute?.IgnoreInterfaces?.All(x => x != baseType) is true or null &&
            (includeDiffAssembly || Equals(baseType.Assembly, typeAssembly)))
        {
            StaticLogger.Trace($"Registering base type {baseType.SaneFullName()}");
            bindTypes.Add(baseType);
        }

        // register with all interfaces
        foreach (var @interface in interfaces)
        {
            if (!(includeDiffAssembly || Equals(@interface.Assembly, typeAssembly)))
                continue;

            StaticLogger.Trace($"Registering interface {@interface.SaneFullName()}");
            bindTypes.Add(@interface);
        }

        // now go through inheritance
        var inheritanceType = baseType?.BaseType;

        while (inheritanceType is not null && inheritanceType != typeof(object))
        {
            // base type
            if (includeDiffAssembly || Equals(inheritanceType.Assembly, typeAssembly))
            {
                bindTypes.Add(inheritanceType);
                StaticLogger.Trace($"Registering base type {inheritanceType.SaneFullName()}");
            }

            inheritanceType = inheritanceType.BaseType;
        }

        // register with type itself
        bindTypes.Add(Type);

        var config = container.RawKernel.Bind(bindTypes.ToArray()).To(Type);

        if (RegisterAttribute is SingletonAttribute)
            config.InSingletonScope();
    }
}