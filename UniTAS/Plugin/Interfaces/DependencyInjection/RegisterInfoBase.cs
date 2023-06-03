using System;
using StructureMap;
using UniTAS.Plugin.Models.DependencyInjection;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

public abstract class RegisterInfoBase
{
    public Type Type { get; protected set; }

    protected RegisterInfoBase(RegisterPriority priority)
    {
        Priority = (int)priority;
    }

    public int Priority { get; }

    public abstract void Register(ConfigurationExpression config);
}