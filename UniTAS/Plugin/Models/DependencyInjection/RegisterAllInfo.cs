using System;
using StructureMap;
using UniTAS.Plugin.Interfaces.DependencyInjection;

namespace UniTAS.Plugin.Models.DependencyInjection;

public class RegisterAllInfo : RegisterInfoBase
{
    public Type Type { get; }
    public Type InnerType { get; }

    public RegisterAllInfo(Type type, Type innerType, RegisterAttribute registerAttribute) : base(registerAttribute
        .Priority)
    {
        Type = type;
        InnerType = innerType;
    }

    public override void Register(ConfigurationExpression config)
    {
        config.For(Type).Use(InnerType);
    }
}