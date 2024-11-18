using System;
using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.DependencyInjection;

/// <summary>
/// Register all classes that inherit from this
/// If the registering class has an attribute such as singleton, it will be registered as a singleton
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class RegisterAllAttribute(
    RegisterPriority priority = RegisterPriority.Default,
    RegisterTiming timing = RegisterTiming.UnityInit)
    : RegisterAttribute(priority, timing)
{
    public override IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes)
    {
        var types = type.IsInterface
            ? allTypes.Where(x => x.GetInterfaces().Contains(type))
            : allTypes.Where(x => x.IsSubclassOf(type) && !x.IsAbstract);

        // if type is abstract, recursively register inner types
        foreach (var innerType in types)
        {
#if UNIT_TESTS
            var innerTypeAttributes =
                innerType.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);
            if (innerTypeAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute)) continue;
#endif

            yield return new RegisterAllInfo(type, innerType, this);

            var innerRegisterInfos = GetRegisterInfos(innerType, allTypes);
            foreach (var innerRegisterInfo in innerRegisterInfos)
            {
                yield return innerRegisterInfo;
            }
        }
    }
}