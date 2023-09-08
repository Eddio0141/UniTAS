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

[Singleton(timing: RegisterTiming.Entry)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ForceInstantiateTypes : IForceInstantiateTypes
{
    private readonly IContainer _container;
    private readonly ILogger _logger;

    private readonly Dictionary<Assembly, Dictionary<RegisterTiming, List<Type>>> _pendingInstantiations = new();

    public ForceInstantiateTypes(IContainer container, ILogger logger)
    {
        _container = container;
        _logger = logger;
    }

    public void InstantiateTypes<TAssemblyContainingType>(RegisterTiming timing)
    {
        var assembly = typeof(TAssemblyContainingType).Assembly;

        if (!_pendingInstantiations.TryGetValue(assembly,
                out var pendingInstantiations))
        {
            _pendingInstantiations.Add(assembly, new());
            pendingInstantiations = _pendingInstantiations[assembly];

            var timings = Enum.GetValues(typeof(RegisterTiming)).Cast<RegisterTiming>();
            foreach (var timingEnum in timings)
            {
                pendingInstantiations.Add(timingEnum, new());
            }

            var allTypes = AccessTools.GetTypesFromAssembly(assembly);
            foreach (var type in allTypes)
            {
                var attributes = type.GetCustomAttributes(typeof(ForceInstantiateAttribute), false);
                if (attributes.Length == 0) continue;

                attributes = type.GetCustomAttributes(typeof(ExcludeRegisterIfTestingAttribute), false);
                if (attributes.Length > 0 && UnitTestUtils.IsTesting) continue;

                var registerAttribute = type.GetCustomAttributes(typeof(RegisterAttribute), true)
                    .Select(x => (RegisterAttribute)x).FirstOrDefault();

                // this shouldn't happen, but just in case
                if (registerAttribute == null) continue;

                pendingInstantiations[registerAttribute.Timing].Add(type);
            }
        }

        if (!pendingInstantiations.ContainsKey(timing)) return;
        var instantiateTypes = pendingInstantiations[timing];
        pendingInstantiations.Remove(timing);

        foreach (var type in instantiateTypes)
        {
            _logger.LogDebug($"Force instantiating {type.Name}");
            _container.GetInstance(type);
        }

        if (pendingInstantiations.Count == 0)
        {
            _pendingInstantiations.Remove(assembly);
        }
    }
}