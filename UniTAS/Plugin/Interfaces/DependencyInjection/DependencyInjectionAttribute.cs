using System;
using System.Collections.Generic;

namespace UniTAS.Plugin.Interfaces.DependencyInjection;

public abstract class DependencyInjectionAttribute : Attribute
{
    public abstract IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes, bool isTesting);
}