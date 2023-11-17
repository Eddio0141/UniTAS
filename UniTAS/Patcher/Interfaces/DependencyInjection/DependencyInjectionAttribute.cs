using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

[MeansImplicitUse]
public abstract class DependencyInjectionAttribute : Attribute
{
    public abstract IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes, bool isTesting);
}