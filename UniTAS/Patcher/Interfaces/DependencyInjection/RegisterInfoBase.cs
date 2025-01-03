using System;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

public abstract class RegisterInfoBase(RegisterPriority priority, RegisterTiming timing)
{
    public Type Type { get; protected set; }
    public RegisterTiming Timing { get; } = timing;

    public int Priority { get; } = (int)priority;

    public abstract void Register(IContainer container);
}