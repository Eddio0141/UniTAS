using System;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Models.DependencyInjection;

public class RegisterAllInfo : RegisterInfoBase
{
    private Type InnerType { get; }

    public RegisterAllInfo(Type type, Type innerType, RegisterAttribute registerAttribute) : base(registerAttribute
        .Priority, registerAttribute.Timing)
    {
        Type = type;
        InnerType = innerType;
    }

    public override void Register(ConfigurationExpression config)
    {
        config.For(Type).Use(InnerType);
    }
}