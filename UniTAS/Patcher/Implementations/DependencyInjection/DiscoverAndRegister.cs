using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.DependencyInjection;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DiscoverAndRegister : IDiscoverAndRegister
{
    private readonly ILogger _logger;

    public DiscoverAndRegister(ILogger logger)
    {
        _logger = logger;
    }

    public void Register<TAssemblyContainingType>(ConfigurationExpression config)
    {
        var allTypes = AccessTools.GetTypesFromAssembly(typeof(TAssemblyContainingType).Assembly);
        var types = allTypes.Where(x => x.GetCustomAttributes(typeof(DependencyInjectionAttribute), true).Length > 0);

        var registers = new List<RegisterInfoBase>();

        foreach (var type in types)
        {
            registers.AddRange(GetRegisterInfos(type, allTypes));
        }

        // order by priority
        registers = registers.OrderBy(x => x.Priority).ToList();

        _logger.LogDebug($"registering {registers.Count} types");
        foreach (var register in registers)
        {
            _logger.LogDebug($"registering {register.Type.FullName} with priority {register.Priority}");
            register.Register(config);
        }
    }

    private IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes)
    {
        var dependencyInjectionAttributes = type.GetCustomAttributes(typeof(DependencyInjectionAttribute), true);

        // early return if ExcludeRegisterIfTestingAttribute is present
        if (UnitTestUtils.IsTesting &&
            dependencyInjectionAttributes.Any(x => x is ExcludeRegisterIfTestingAttribute)) yield break;

        foreach (var dependencyInjectionAttribute in dependencyInjectionAttributes)
        {
            var registerAttribute = (DependencyInjectionAttribute)dependencyInjectionAttribute;
            var infos = registerAttribute.GetRegisterInfos(type, allTypes, UnitTestUtils.IsTesting);
            foreach (var info in infos)
            {
                yield return info;
            }
        }
    }
}