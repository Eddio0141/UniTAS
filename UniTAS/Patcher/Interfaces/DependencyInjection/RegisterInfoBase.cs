using System;
using StructureMap;
using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

public abstract class RegisterInfoBase
{
    public Type Type { get; protected set; }
    public RegisterTiming Timing { get; }

    protected RegisterInfoBase(RegisterPriority priority, RegisterTiming timing)
    {
        Priority = (int)priority;
        Timing = timing;
    }

    public int Priority { get; }

    public abstract void Register(ConfigurationExpression config);
}