using System;
using System.Collections.Generic;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

/// <summary>
/// Explicitly exclude a class from being registered when unit testing
/// </summary>
public class ExcludeRegisterIfTestingAttribute : DependencyInjectionAttribute
{
    // TODO: should eventually remove this
    public override IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes)
    {
        yield break;
    }
}