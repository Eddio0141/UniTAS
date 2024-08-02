using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

/// <summary>
/// Use this to register a class as a singleton
/// The interfaces has to be in the same assembly as the class to be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute(
    RegisterPriority priority = RegisterPriority.Default,
    RegisterTiming timing = RegisterTiming.UnityInit)
    : RegisterAttribute(priority, timing)
{
    public override IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes, bool isTesting)
    {
        yield return new RegisterInfo(type, this, Timing);
    }
}