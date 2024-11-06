using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.DependencyInjection;

[Singleton(timing: RegisterTiming.Entry)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ForceInstantiateTypes(IContainer container, ILogger logger) : IForceInstantiateTypes
{
    private readonly Dictionary<Assembly, Dictionary<RegisterTiming, List<Type>>> _pendingInstantiations = new();

    public void InstantiateTypes<TAssemblyContainingType>(RegisterTiming timing)
    {
        using var _ = Bench.Measure();
        var assembly = typeof(TAssemblyContainingType).Assembly;

        if (!_pendingInstantiations.TryGetValue(assembly,
                out var pendingInstantiations))
        {
            _pendingInstantiations.Add(assembly, new());
            pendingInstantiations = _pendingInstantiations[assembly];

            var pending = new Dictionary<RegisterTiming, List<(Type, RegisterPriority)>>();
            var timings = Enum.GetValues(typeof(RegisterTiming)).Cast<RegisterTiming>();
            foreach (var timingEnum in timings)
            {
                pending.Add(timingEnum, new());
            }

            var allTypes = AccessTools.GetTypesFromAssembly(assembly);
            foreach (var type in allTypes)
            {
                var attributes = type.GetCustomAttributes(typeof(ForceInstantiateAttribute), true);
                if (attributes.Length == 0) continue;

                attributes = type.GetCustomAttributes(typeof(ExcludeRegisterIfTestingAttribute), false);
                if (attributes.Length > 0 && UnitTestUtils.IsTesting) continue;

                var registerAttribute = type.GetCustomAttributes(typeof(RegisterAttribute), true)
                    .Select(x => (RegisterAttribute)x).FirstOrDefault();

                // this shouldn't happen, but just in case
                if (registerAttribute == null) continue;

                pending[registerAttribute.Timing].Add((type, registerAttribute.Priority));
            }

            // sort by priority
            foreach (var key in pending.Keys)
            {
                pendingInstantiations[key] = pending[key].OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            }
        }

        if (!pendingInstantiations.TryGetValue(timing, out var instantiateTypes)) return;
        pendingInstantiations.Remove(timing);

        foreach (var type in instantiateTypes)
        {
            logger.LogDebug($"Force instantiating {type.Name}");
            container.GetInstance(type);
        }

        if (pendingInstantiations.Count == 0)
        {
            _pendingInstantiations.Remove(assembly);
        }
    }
}