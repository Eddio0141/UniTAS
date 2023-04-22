using System;
using System.Collections.Generic;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

public class ForceInstantiateAttribute : DependencyInjectionAttribute
{
    public override IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes, bool isTesting)
    {
        yield break;
    }
}