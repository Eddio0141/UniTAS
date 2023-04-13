using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.RuntimeTest;
using UniTAS.Plugin.Services.RuntimeTest;
using UniTAS.Plugin.Utils;

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

        OnDiscoveredTests?.Invoke(testMethods.Count);

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

            OnTestRun?.Invoke(testName);

            try
            {
                var instance = test.IsStatic ? null : instances.FirstOrDefault(x => x.GetType() == test.DeclaringType);
                var ret = test.Invoke(instance, emptyParams);

                // check if skipped test
                if (ExtractReturnType<bool>(ret) is false)
                {
                    testResults.Add(new(testName));
                    continue;
                }

                testResults.Add(new(testName, true));
            }
            catch (Exception e)
            {
                testResults.Add(new(testName, false, e));
            }
        }

        return testResults;
    }

    private static object ExtractReturnType<T>(object returnValue)
    {
        if (returnValue is T) return returnValue;

        var returnType = returnValue?.GetType();
        if (returnType?.FullName == null ||
            !returnType.FullName.StartsWith($"{typeof(Tuple<,>).Namespace}.Tuple`")) return null;

        var fields = AccessTools.GetDeclaredFields(returnType);
        foreach (var field in fields)
        {
            if (!field.Name.StartsWith("Item")) continue;

            var value = field.GetValue(returnValue);
            if (value.GetType() == typeof(T)) return value;
        }

        return null;
    }

    public event DiscoveredTests OnDiscoveredTests;
    public event TestRun OnTestRun;
}