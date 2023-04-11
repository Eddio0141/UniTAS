using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.RuntimeTest;
using UniTAS.Plugin.Services.RuntimeTest;

namespace UniTAS.Plugin.Implementations.RuntimeTest;

[Singleton]
public class RuntimeTestProcessor : IRuntimeTestProcessor
{
    private readonly IContainer _container;

    public RuntimeTestProcessor(IContainer container)
    {
        _container = container;
    }

    public List<TestResult> Test<T>()
    {
        var typeAssembly = typeof(T).Assembly;
        var types = AccessTools.GetTypesFromAssembly(typeAssembly);
        var testMethods = types.SelectMany(AccessTools.GetDeclaredMethods)
            .Where(m => m.GetCustomAttributes(false).Any(x => x is RuntimeTestAttribute)).ToList();
        var instances =
            testMethods.Where(m => !m.IsStatic && m.DeclaringType is { IsAbstract: false, IsInterface: false })
                .Select(m => m.DeclaringType)
                .Distinct()
                .Select(t => _container.TryGetInstance(t) ?? AccessTools.CreateInstance(t))
                .ToList();
        var testResults = new List<TestResult>();
        var emptyParams = new object[0];

        foreach (var test in testMethods)
        {
            var typeName = test.DeclaringType?.Name ?? string.Empty;
            var testName = $"{typeName}.{test.Name}";

            try
            {
                var instance = test.IsStatic ? null : instances.FirstOrDefault(x => x.GetType() == test.DeclaringType);
                test.Invoke(instance, emptyParams);
                testResults.Add(new(testName, true));
            }
            catch (Exception e)
            {
                testResults.Add(new(testName, false, e));
            }
        }

        return testResults;
    }
}