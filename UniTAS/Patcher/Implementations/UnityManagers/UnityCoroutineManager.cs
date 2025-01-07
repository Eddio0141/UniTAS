using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityManagers;

[Singleton]
public class UnityCoroutineManager : ICoroutineTracker
{
    private readonly HashSet<MonoBehaviour> _instances = [];
    private readonly HashSet<Type> _patchedCoroutines = [];

    public void NewCoroutine(object instance, IEnumerator routine)
    {
        NewCoroutine(instance as MonoBehaviour, routine);
    }

    public void NewCoroutine(MonoBehaviour instance, IEnumerator routine)
    {
        if (instance == null) return;
        if (routine == null)
        {
            _logger.LogWarning($"coroutine is null, {new StackTrace()}");
            return;
        }

        // don't track ours
        if (Equals(instance.GetType().Assembly, typeof(UnityCoroutineManager).Assembly)) return;

        if (!_instances.Contains(instance))
            _instances.Add(instance);

        var routineType = routine.GetType();
        _logger.LogDebug(
            $"new coroutine made in script {instance.GetType().SaneFullName()}, got IEnumerator {routineType.SaneFullName()}");
        StaticLogger.Trace($"call from {new StackTrace()}");

        if (_patchedCoroutines.Contains(routineType)) return;
        _patchedCoroutines.Add(routineType);
        _logger.LogDebug("this coroutine is yet to be patched");

        var current =
            AccessTools.PropertyGetter(routineType, $"{typeof(IEnumerator).FullName}.{nameof(IEnumerator.Current)}") ??
            AccessTools.PropertyGetter(routineType, nameof(IEnumerator.Current));
        var moveNext =
            AccessTools.Method(routineType, $"{typeof(IEnumerator).FullName}.{nameof(IEnumerator.MoveNext)}") ??
            AccessTools.Method(routineType, nameof(IEnumerator.MoveNext));
        _harmony.Harmony.Patch(current, postfix: CurrentPostfix);
        _harmony.Harmony.Patch(moveNext, MoveNextPrefix);
    }

    private readonly Dictionary<Type, HashSet<MethodBase>> _patchedCoroutinesMethods = [];
    private readonly ILogger _logger;
    private readonly IHarmony _harmony;

    public UnityCoroutineManager(ILogger logger, IHarmony harmony, IGameRestart gameRestart)
    {
        _logger = logger;
        _harmony = harmony;
        // do not use interface for game restart, it fucks everything up
        gameRestart.OnPreGameRestart += OnPreGameRestart;
    }

    public void NewCoroutine(MonoBehaviour instance, string methodName, object value)
    {
        // find target method
        var method = AccessTools.GetDeclaredMethods(instance.GetType())
            .FirstOrDefault(x => !x.IsStatic && x.Name == methodName);
        if (method == null) return;
        var requiredArgs = value == null ? 0 : 1;
        if (method.GetParameters().Length != requiredArgs) return;
        var instanceType = instance.GetType();
        if (_patchedCoroutinesMethods.TryGetValue(instanceType, out var coroutineMethods))
        {
            if (coroutineMethods.Contains(method)) return;
            coroutineMethods.Add(method);
        }
        else
        {
            _patchedCoroutinesMethods[instanceType] = [method];
        }

        _harmony.Harmony.Patch(method, postfix: NewCoroutinePostfixMethod);
    }

    private void NewCoroutineHandle(MonoBehaviour instance, Type routineType)
    {
        if (instance == null) return;
        // don't track ours
        if (Equals(instance.GetType().Assembly, typeof(UnityCoroutineManager).Assembly)) return;

        _logger.LogDebug(
            $"new coroutine made in script {instance.GetType().SaneFullName()}, got IEnumerator {routineType.SaneFullName()}");
        StaticLogger.Trace($"call from {new StackTrace()}");

        _instances.Add(instance);

        if (_patchedCoroutines.Contains(routineType)) return;
        _patchedCoroutines.Add(routineType);
        _logger.LogDebug("this coroutine is yet to be patched");

        var current =
            AccessTools.PropertyGetter(routineType, $"{typeof(IEnumerator).FullName}.{nameof(IEnumerator.Current)}") ??
            AccessTools.PropertyGetter(routineType, nameof(IEnumerator.Current));
        var moveNext =
            AccessTools.Method(routineType, nameof(IEnumerator.MoveNext), [typeof(bool)]) ??
            AccessTools.Method(routineType, $"{typeof(IEnumerator).FullName}.{nameof(IEnumerator.MoveNext)}",
                [typeof(bool)]);
        _harmony.Harmony.Patch(current, postfix: CurrentPostfix);
        _harmony.Harmony.Patch(moveNext, MoveNextPrefix);
    }

    private void OnPreGameRestart()
    {
        foreach (var coroutine in _instances)
        {
            if (coroutine == null) continue;
            coroutine.StopAllCoroutines();
        }

        _instances.Clear();
        // no need, but better clean it up
        DoneFirstMoveNext.Clear();
    }

    private static readonly HarmonyMethod CurrentPostfix =
        new(typeof(UnityCoroutineManager), nameof(CoroutineCurrentPostfix));

    private static readonly HarmonyMethod MoveNextPrefix =
        new(typeof(UnityCoroutineManager), nameof(CoroutineMoveNextPrefix));

    private static readonly HarmonyMethod NewCoroutinePostfixMethod =
        new(typeof(UnityCoroutineManager), nameof(NewCoroutinePostfix));

    private static readonly IMonoBehaviourController MonoBehaviourController =
        ContainerStarter.Kernel.GetInstance<IMonoBehaviourController>();

    private static readonly IAsyncOperationTracker AsyncOperationTracker =
        ContainerStarter.Kernel.GetInstance<IAsyncOperationTracker>();

    private static readonly IPatchReverseInvoker ReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    // ReSharper disable InconsistentNaming
    // TODO: for both, handle time based coroutines and check the rest

    private class YieldNone : IEnumerator
    {
        public bool MoveNext() => false;

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public object Current => null;
    }

    private static readonly YieldNone NoYield = new();

    private static readonly IMonoBehEventInvoker MonoBehEventInvoker =
        ContainerStarter.Kernel.GetInstance<IMonoBehEventInvoker>();

    private static readonly IAsyncOperationOverride AsyncOperationOverride =
        ContainerStarter.Kernel.GetInstance<IAsyncOperationOverride>();

    private static void CoroutineCurrentPostfix(ref object __result)
    {
        if (__result == null) return;
        if (ReverseInvoker.Invoking) return;

        // managed async operation?
        if (__result is AsyncOperation op && AsyncOperationTracker.ManagedInstance(op))
        {
            if (op.isDone)
            {
                __result = NoYield;
                return;
            }

            StaticLogger.Trace("paused execution for AsyncOperation Current, result is managed by unitas" +
                               $", and it isn't complete, replaced result with null: {new StackTrace()}");
            __result = null;
            return;
        }

        if (MonoBehaviourController.PausedExecution)
        {
            // TODO: i can probably just manually check for CoreModule / UnityEngine since its not like there's many of this
            if (__result.GetType().Assembly.GetName().Name.StartsWith("UnityEngine"))
            {
                StaticLogger.Trace("paused execution for coroutine Current" +
                                   $", result is unity type: {__result.GetType().SaneFullName()}" +
                                   $", replaced result with null: {new StackTrace()}");
                __result = null;
            }

            return;
        }

        if (__result is WaitForEndOfFrame)
        {
            // MoveNext is invoked first, so code already ran, just run this here
            MonoBehEventInvoker.InvokeEndOfFrame();

            if (MonoBehaviourController.PausedUpdate)
            {
                StaticLogger.Trace("paused update execution for coroutine Current" +
                                   $", result is type: {__result.GetType().SaneFullName()}" +
                                   $", replaced result with null: {new StackTrace()}");
                __result = null;
                // return;
            }
        }
    }

    private static readonly HashSet<IEnumerator> DoneFirstMoveNext = [];

    private static bool CoroutineMoveNextPrefix(IEnumerator __instance, ref bool __result)
    {
        if (!DoneFirstMoveNext.Contains(__instance))
        {
            DoneFirstMoveNext.Add(__instance);
            return true;
        }

        var current = ReverseInvoker.Invoke(i => i.Current, __instance);

        // managed async operation?
        if (current is AsyncOperation op && AsyncOperationOverride.Yield(op))
        {
            var isDone = op.isDone;
            StaticLogger.Trace("coroutine MoveNext with AsyncOperation, operation is managed by unitas" +
                               $", running MoveNext: {isDone}");
            if (!isDone)
                __result = true;
            return isDone;
        }

        if (MonoBehaviourController.PausedExecution)
        {
            if (current is null)
            {
                StaticLogger.Trace("paused execution while coroutine MoveNext, Current is null, not running MoveNext");
                __result = true;
                return false;
            }

            // TODO: i can probably just manually check for CoreModule / UnityEngine since its not like there's many of this
            if (current.GetType().Assembly.GetName().Name.StartsWith("UnityEngine"))
            {
                StaticLogger.Trace("paused execution while coroutine MoveNext" +
                                   $", Current is type: {current.GetType().SaneFullName()}" +
                                   " unity type, not running MoveNext");
                __result = true;
                return false;
            }

            return true;
        }

        if (MonoBehaviourController.PausedUpdate && current is null or WaitForEndOfFrame)
        {
            StaticLogger.Trace("paused update execution while coroutine MoveNext" +
                               ", Current is null / WaitForEndOfFrame, not running MoveNext");
            __result = true;
            return false;
        }

        return true;
    }

    private static readonly HashSet<MethodBase> _newCoroutineRan = [];

    private static readonly UnityCoroutineManager CoroutineManager =
        ContainerStarter.Kernel.GetInstance<UnityCoroutineManager>();

    private static void NewCoroutinePostfix(MonoBehaviour __instance, IEnumerator __result, MethodBase __originalMethod)
    {
        if (__result == null) return;
        if (_newCoroutineRan.Contains(__originalMethod)) return;
        _newCoroutineRan.Add(__originalMethod);
        CoroutineManager.NewCoroutineHandle(__instance, __result.GetType());
    }

    // ReSharper restore InconsistentNaming
}