using System;
using Ninject;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Utils;

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

    public override void Register(IContainer container)
    {
        container.RawKernel.Bind(Type).To(InnerType);
    }
}