using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StructureMap;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Models.Coroutine;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.RuntimeTest;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.RuntimeTest;

namespace UniTAS.Patcher.Implementations.RuntimeTest;

[Singleton(RegisterPriority.RuntimeTestProcessor)]
public class RuntimeTestProcessor : IRuntimeTestProcessor
{
    private readonly IContainer _container;
    private readonly ICoroutine _coroutine;

    private string _processingCoroutineName;
    private readonly Queue<Models.Utils.Tuple<string, IEnumerable<CoroutineWait>>> _pendingCoroutines = new();
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
            .Select(t =>
            {
                try
                {
                    return _container.GetInstance(t);
                }
                catch (Exception)
                {
                    return AccessTools.CreateInstance(t);
                }
            })
            .ToList();
        var emptyParams = new object[0];

        foreach (var test in testMethods)
        {
            var typeName = test.DeclaringType?.Name ?? string.Empty;
            var testName = $"{typeName}.{test.Name}";

            // test run event unless return type is coroutine
            if (!MethodHasTypeReturn<IEnumerable<CoroutineWait>>(test))
            {
                OnTestRun?.Invoke(testName);
            }

            object ret;
            try
            {
                var instance = test.IsStatic ? null : instances.FirstOrDefault(x => x.GetType() == test.DeclaringType);
                ret = test.Invoke(instance, emptyParams);
            }
            catch (Exception e)
            {
                TestEnd(new(testName, false, e));
                continue;
            }

            // check if skipped test
            if (ExtractReturnType<bool>(ret, out var notSkippedTest) && !notSkippedTest)
            {
                TestEnd(new(testName));
                continue;
            }

            if (ExtractReturnType<IEnumerable<CoroutineWait>>(ret, out var coroutine))
            {
                _pendingCoroutines.Enqueue(new(testName, coroutine));
                continue;
            }

            TestEnd(new(testName, true));
        }

        if (_pendingCoroutines.Count > 0)
        {
            RunNextCoroutine();
            return;
        }

        OnTestsFinish?.Invoke(new(_testResults));
    }

    private void RunNextCoroutine()
    {
        if (_pendingCoroutines.Count == 0) return;
        var next = _pendingCoroutines.Dequeue();

        _processingCoroutineName = next.Item1;

        var coroutineStatus = _coroutine.Start(next.Item2);
        coroutineStatus.OnComplete += CoroutineTestEnd;

        OnTestRun?.Invoke(_processingCoroutineName);
    }

    private void CoroutineTestEnd(CoroutineStatus status)
    {
        if (status.Exception != null)
        {
            TestEnd(new(_processingCoroutineName, false, status.Exception));
        }
        else
        {
            TestEnd(new(_processingCoroutineName, true));
        }

        if (_pendingCoroutines.Count > 0)
        {
            RunNextCoroutine();
            return;
        }

        OnTestsFinish?.Invoke(new(_testResults));
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
            !returnType.FullName.StartsWith($"{typeof(Models.Utils.Tuple<,>).Namespace}.Tuple`")) return false;

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

    private static bool MethodHasTypeReturn<T>(MethodInfo method)
    {
        var returnType = method.ReturnType;

        if (returnType == typeof(T)) return true;
        if (returnType.FullName == null ||
            !returnType.FullName.StartsWith($"{typeof(Models.Utils.Tuple<,>).Namespace}.Tuple`")) return false;

        var fields = AccessTools.GetDeclaredFields(returnType);
        foreach (var field in fields)
        {
            if (!field.Name.StartsWith("<Item")) continue;

            if (field.FieldType == typeof(T))
            {
                return true;
            }
        }

        return false;
    }

    private void TestEnd(TestResult result)
    {
        _testResults.Add(result);
        OnTestEnd?.Invoke(result);
    }

    public event DiscoveredTests OnDiscoveredTests;
    public event TestRun OnTestRun;
    public event TestsFinish OnTestsFinish;
    public event TestEnd OnTestEnd;
}