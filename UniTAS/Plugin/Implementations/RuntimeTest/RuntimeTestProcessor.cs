using System;
using System.Collections.Generic;
using System.Linq;
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

    private readonly List<Tuple<string, CoroutineStatus>> _coroutineStatuses = new();
    private readonly List<TestResult> _testResults = new();

    public RuntimeTestProcessor(IContainer container, ICoroutine coroutine)
    {
        _container = container;
        _coroutine = coroutine;
    }

    public void Test<T>()
    {
        _testResults.Clear();
        _coroutineStatuses.Clear();

        var typeAssembly = typeof(T).Assembly;
        var types = AccessTools.GetTypesFromAssembly(typeAssembly);
        var testMethods = types.SelectMany(AccessTools.GetDeclaredMethods)
            .Where(m => m.GetCustomAttributes(false).Any(x => x is RuntimeTestAttribute)).ToList();

        OnDiscoveredTests?.Invoke(testMethods.Count);

        var instances = testMethods
            .Where(m => !m.IsStatic && m.DeclaringType is { IsAbstract: false, IsInterface: false })
            .Select(m => m.DeclaringType)
            .Distinct()
            .Select(t => _container.TryGetInstance(t) ?? AccessTools.CreateInstance(t))
            .ToList();
        var emptyParams = new object[0];

        foreach (var test in testMethods)
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
                _testResults.Add(new(testName, false, e));
                continue;
            }

            // check if skipped test
            if (ExtractReturnType<bool>(ret, out var skippedTest) && !skippedTest)
            {
                _testResults.Add(new(testName));
                continue;
            }

            if (ExtractReturnType<IEnumerator<CoroutineWait>>(ret, out var coroutine))
            {
                var coroutineStatus = _coroutine.Start(coroutine);
                coroutineStatus.OnComplete += CoroutineTestEnd;
                _coroutineStatuses.Add(new(testName, coroutineStatus));
                continue;
            }

            _testResults.Add(new(testName, true));
        }
    }

    private void CoroutineTestEnd(CoroutineStatus status)
    {
        status.OnComplete -= CoroutineTestEnd;
        var storedStatus = _coroutineStatuses.FirstOrDefault(x => x.Item2 == status);
        _coroutineStatuses.Remove(storedStatus);

        if (storedStatus == null)
        {
            throw new NullReferenceException("CoroutineStatus not found in list");
        }

        var testName = storedStatus.Item1;
        if (status.Exception != null)
        {
            _testResults.Add(new(testName, false, status.Exception));
        }
        else
        {
            _testResults.Add(new(testName, true));
        }

        if (_coroutineStatuses.Count > 0) return;

        OnTestEnd?.Invoke(_testResults);
    }

    private static bool ExtractReturnType<T>(object returnValue, out T retValue)
    {
        retValue = default;

        switch (returnValue)
        {
            case null:
                return false;
            case T t:
                retValue = t;
                return true;
        }

        var returnType = returnValue.GetType();
        if (returnType.FullName == null ||
            !returnType.FullName.StartsWith($"{typeof(Tuple<,>).Namespace}.Tuple`")) return false;

        var fields = AccessTools.GetDeclaredFields(returnType);
        foreach (var field in fields)
        {
            if (!field.Name.StartsWith("<Item")) continue;

            var value = field.GetValue(returnValue);
            if (value is T valueT)
            {
                retValue = valueT;
                return true;
            }
        }

        return false;
    }

    public event DiscoveredTests OnDiscoveredTests;
    public event TestRun OnTestRun;
    public event TestEnd OnTestEnd;
}