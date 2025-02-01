using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.DependencyInjection;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DiscoverAndRegister(ILogger logger) : IDiscoverAndRegister
{
    public void Register<TAssemblyContainingType>(ConfigurationExpression config)
    {
        var allTypes = AccessTools.GetTypesFromAssembly(typeof(TAssemblyContainingType).Assembly);

        var types = allTypes.Where(
            x => x.GetCustomAttributes(typeof(DependencyInjectionAttribute), true).Length > 0);

        logger.LogDebug("registering types");
        var count = 0;
        foreach (var type in types)
        {
            foreach (var register in GetRegisterInfos(type, allTypes).OrderBy(x => x.Priority))
            {
                logger.LogDebug($"registering {register.Type.FullName} with priority {register.Priority}");
                register.Register(config);
                count++;
            }
        }

        logger.LogDebug($"registered {count} types");
    }

    private static IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes)
    {
        var dependencyInjectionAttributes = type.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);

        // early return if ExcludeRegisterIfTestingAttribute is present
#if UNIT_TESTS
        if (dependencyInjectionAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute)) yield break;
#endif

        foreach (var dependencyInjectionAttribute in dependencyInjectionAttributes)
        {
            var registerAttribute = (DependencyInjectionAttribute)dependencyInjectionAttribute;
            var infos = registerAttribute.GetRegisterInfos(type, allTypes);
            foreach (var info in infos)
            {
                yield return info;
            }
        }
    }
}