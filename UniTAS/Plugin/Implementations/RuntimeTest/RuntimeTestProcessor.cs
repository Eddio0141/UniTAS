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

    private string _processingCoroutineName;
    private readonly Queue<Tuple<string, IEnumerator<CoroutineWait>>> _pendingCoroutines = new();
    private readonly List<TestResult> _testResults = new();

    public RuntimeTestProcessor(IContainer container, ICoroutine coroutine)
    {
        _container = container;
        _coroutine = coroutine;
    }

    public void Test<T>()
    {
        if (_pendingCoroutines.Count > 0)
        {
            throw new InvalidOperationException("Cannot run test while another test is running");
        }

        _testResults.Clear();
        _pendingCoroutines.Clear();

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
                _pendingCoroutines.Enqueue(new(testName, coroutine));
                continue;
            }

            _testResults.Add(new(testName, true));
        }

        if (_pendingCoroutines.Count > 0)
        {
            RunNextCoroutine();
            return;
        }

        OnTestEnd?.Invoke(_testResults);
    }

    private void RunNextCoroutine()
    {
        if (_pendingCoroutines.Count == 0) return;
        var next = _pendingCoroutines.Dequeue();

        _processingCoroutineName = next.Item1;

        var coroutineStatus = _coroutine.Start(next.Item2);
        coroutineStatus.OnComplete += CoroutineTestEnd;
    }

    private void CoroutineTestEnd(CoroutineStatus status)
    {
        status.OnComplete -= CoroutineTestEnd;

        if (status.Exception != null)
        {
            _testResults.Add(new(_processingCoroutineName, false, status.Exception));
        }
        else
        {
            _testResults.Add(new(_processingCoroutineName, true));
        }

        if (_pendingCoroutines.Count > 0)
        {
            RunNextCoroutine();
            return;
        }

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