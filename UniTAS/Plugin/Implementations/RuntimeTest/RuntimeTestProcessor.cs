using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Models.RuntimeTest;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.RuntimeTest;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Implementations.RuntimeTest;

[Singleton]
public class RuntimeTestProcessor : IRuntimeTestProcessor
{
    private readonly IContainer _container;
    private readonly ICoroutine _coroutine;

    public RuntimeTestProcessor(IContainer container, ICoroutine coroutine)
    {
        _container = container;
        _coroutine = coroutine;
    }

    public void Test<T>()
    {
        var typeAssembly = typeof(T).Assembly;
        var types = AccessTools.GetTypesFromAssembly(typeAssembly);
        var testMethods = types.SelectMany(AccessTools.GetDeclaredMethods)
            .Where(m => m.GetCustomAttributes(false).Any(x => x is RuntimeTestAttribute)).ToList();

        OnDiscoveredTests?.Invoke(testMethods.Count);

        _coroutine.Start(TestCoroutine(testMethods));
    }

    private IEnumerator<CoroutineWait> TestCoroutine(List<MethodInfo> tests)
    {
        var instances = tests.Where(m => !m.IsStatic && m.DeclaringType is { IsAbstract: false, IsInterface: false })
            .Select(m => m.DeclaringType)
            .Distinct()
            .Select(t => _container.TryGetInstance(t) ?? AccessTools.CreateInstance(t))
            .ToList();
        var emptyParams = new object[0];
        var testResults = new List<TestResult>();

        foreach (var test in tests)
        {
            var typeName = test.DeclaringType?.Name ?? string.Empty;
            var testName = $"{typeName}.{test.Name}";

            OnTestRun?.Invoke(testName);

            object ret;
            try
            {
                var instance = test.IsStatic ? null : instances.FirstOrDefault(x => x.GetType() == test.DeclaringType);
                ret = test.Invoke(instance, emptyParams);
            }
            catch (Exception e)
            {
                testResults.Add(new(testName, false, e));
                continue;
            }

            var coroutine = ExtractReturnType<IEnumerator<CoroutineWait>>(ret);
            var coroutineStatus = _coroutine.Start(coroutine);
            if (coroutine != null) yield return new WaitForCoroutine(coroutineStatus);

            // check if coroutine threw exception
            if (coroutineStatus.Exception != null)
            {
                testResults.Add(new(testName, false, coroutineStatus.Exception));
                continue;
            }

            // check if skipped test
            if (ExtractReturnType<bool>(ret) is false)
            {
                testResults.Add(new(testName));
                continue;
            }

            testResults.Add(new(testName, true));
        }

        OnTestEnd?.Invoke(testResults);
    }

    private static T ExtractReturnType<T>(object returnValue)
    {
        if (returnValue is T t) return t;

        var returnType = returnValue?.GetType();
        if (returnType?.FullName == null ||
            !returnType.FullName.StartsWith($"{typeof(Tuple<,>).Namespace}.Tuple`")) return default;

        var fields = AccessTools.GetDeclaredFields(returnType);
        foreach (var field in fields)
        {
            if (!field.Name.StartsWith("<Item")) continue;

            var value = field.GetValue(returnValue);
            if (value is T valueT) return valueT;
        }

        return default;
    }

    public event DiscoveredTests OnDiscoveredTests;
    public event TestRun OnTestRun;
    public event TestEnd OnTestEnd;
}