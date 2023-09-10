using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.DependencyInjection;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class DiscoverAndRegister : IDiscoverAndRegister
{
    private readonly ILogger _logger;

    private readonly Dictionary<Assembly, Dictionary<RegisterTiming, List<RegisterInfoBase>>> _pendingRegisters = new();

    public DiscoverAndRegister(ILogger logger)
    {
        _logger = logger;
    }

    public void Register<TAssemblyContainingType>(ConfigurationExpression config, RegisterTiming timing)
    {
        var assembly = typeof(TAssemblyContainingType).Assembly;

        // have we processed pending registers yet?
        if (!_pendingRegisters.TryGetValue(assembly, out var pendingRegisters))
        {
            pendingRegisters = new();
            _pendingRegisters.Add(assembly, pendingRegisters);

            var allTypes = AccessTools.GetTypesFromAssembly(typeof(TAssemblyContainingType).Assembly);
            var types = allTypes.Where(
                x => x.GetCustomAttributes(typeof(DependencyInjectionAttribute), true).Length > 0);

            var registers = new List<RegisterInfoBase>();

            foreach (var type in types)
            {
                registers.AddRange(GetRegisterInfos(type, allTypes));
            }

            // order by priority
            registers = registers.OrderBy(x => x.Priority).ToList();

            // group by timing
            var timings = Enum.GetValues(typeof(RegisterTiming)).Cast<RegisterTiming>();
            foreach (var registerTiming in timings)
            {
                pendingRegisters.Add(registerTiming, new());
            }

            foreach (var register in registers)
            {
                pendingRegisters[register.Timing].Add(register);
            }
        }

        // actually register
        if (!pendingRegisters.TryGetValue(timing, out var currentRegisters)) return;
        pendingRegisters.Remove(timing);

        _logger.LogDebug($"registering {currentRegisters.Count} types with timing {timing}");
        foreach (var register in currentRegisters)
        {
            _logger.LogDebug($"registering {register.Type.FullName} with priority {register.Priority}");
            register.Register(config);
        }

        if (pendingRegisters.Count == 0)
        {
            _pendingRegisters.Remove(assembly);
        }
    }

    private static IEnumerable<RegisterInfoBase> GetRegisterInfos(Type type, Type[] allTypes)
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