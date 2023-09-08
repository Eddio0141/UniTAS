using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

/// <summary>
/// Classes that inherit from this will be registered as a dependency related with the interface
/// The interfaces has to be in the same assembly as the class to be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterAttribute : DependencyInjectionAttribute
{
    public bool IncludeDifferentAssembly { get; set; }

    // public Type[] IgnoreInterfaces { get; set; }
    public RegisterPriority Priority { get; }
    public RegisterTiming Timing { get; }

    public RegisterAttribute(RegisterPriority priority = RegisterPriority.Default,
        RegisterTiming timing = RegisterTiming.UnityInit)
    {
        Priority = priority;
        Timing = timing;
    }

    public override IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes, bool isTesting)
    {
        yield return new RegisterInfo(type, this, Timing);
    }
}