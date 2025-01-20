using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
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
public class UnityCoroutineManager : ICoroutineTracker, IOnUpdateUnconditional
{
    private readonly HashSet<MonoBehaviour> _instances = [];
    private readonly HashSet<IEnumerator> _enumeratorInstances = [];
    private readonly HashSet<IEnumerator> _enumeratorRunningInstances = [];
    private readonly HashSet<IEnumerator> _coroutineRanThisFrame = [];

    public void NewCoroutine(object instance, IEnumerator routine) =>
        NewCoroutineHandle(instance as MonoBehaviour, routine);

    public void NewCoroutine(MonoBehaviour instance, IEnumerator routine) => NewCoroutineHandle(instance, routine);

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

    public void UpdateUnconditional()
    {
        _coroutineRanThisFrame.Clear();
        CoroutinesFinishedThisFrame = _enumeratorRunningInstances.Count == 0;
    }

    public bool CoroutinesFinishedThisFrame { get; private set; } = true;

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

    private void NewCoroutineHandle(MonoBehaviour instance, IEnumerator routine)
    {
        if (instance == null || routine == null) return;
        // don't track ours
        var routineType = routine.GetType();
        if (Equals(routineType.Assembly, typeof(UnityCoroutineManager).Assembly)) return;

        _logger.LogDebug(
            $"new coroutine made in script {instance.GetType().SaneFullName()}, got IEnumerator {routineType.SaneFullName()}");
        StaticLogger.Trace($"call from {new StackTrace()}");

        _instances.Add(instance);
        _enumeratorInstances.Add(routine);
        _enumeratorRunningInstances.Add(routine);
        CoroutinesFinishedThisFrame = false;
    }

    private void OnPreGameRestart()
    {
        foreach (var coroutine in _instances)
        {
            if (coroutine == null) continue;
            coroutine.StopAllCoroutines();
        }

        _instances.Clear();
        _enumeratorInstances.Clear();
        _enumeratorRunningInstances.Clear();
        _coroutineRanThisFrame.Clear();
        // no need, but better clean it up
        DoneFirstMoveNext.Clear();

        CoroutinesFinishedThisFrame = true;
    }

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

    public void CoroutineCurrentPostfix(IEnumerator __instance, ref object __result)
    {
        if (!_enumeratorInstances.Contains(__instance)) return;

        StaticLogger.Trace($"coroutine get_Current: {__instance.GetType().SaneFullName()}");

        if (ReverseInvoker.Invoking) return;
        if (__result == null)
        {
            CoroutineCurrentPostfixFinish(__instance);
            return;
        }

        // managed async operation?
        if (__result is AsyncOperation op && AsyncOperationOverride.Yield(op))
        {
            if (op.isDone)
            {
                __result = NoYield;
                CoroutineCurrentPostfixFinish(__instance);
                return;
            }

            StaticLogger.Trace("paused execution for AsyncOperation Current, result is managed by unitas" +
                               $", and it isn't complete, replaced result with null: {new StackTrace()}");
            __result = null;
            CoroutineCurrentPostfixFinish(__instance);
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

            CoroutineCurrentPostfixFinish(__instance);
            return;
        }

        if (__result is WaitForEndOfFrame)
        {
            MonoBehEventInvoker.InvokeEndOfFrame();

            // do we invoke end of frame?
            if (_coroutineRanThisFrame.Count + 1 == _enumeratorRunningInstances.Count)
            {
                // MoveNext is invoked first, so code already ran, just run this here
                MonoBehEventInvoker.InvokeLastUpdate();
            }

            if (MonoBehaviourController.PausedUpdate)
            {
                StaticLogger.Trace("paused update execution for coroutine Current" +
                                   $", result is type: {__result.GetType().SaneFullName()}" +
                                   $", replaced result with null: {new StackTrace()}");
                __result = null;
                // CoroutineCurrentPostfixFinish(__instance); add if return below is used
                // return;
            }
        }

        CoroutineCurrentPostfixFinish(__instance);
    }

    private void CoroutineCurrentPostfixFinish(IEnumerator __instance)
    {
        _coroutineRanThisFrame.Add(__instance);
        if (_coroutineRanThisFrame.Count == _enumeratorRunningInstances.Count)
            CoroutinesFinishedThisFrame = true;
    }

    private static readonly HashSet<IEnumerator> DoneFirstMoveNext = [];

    // state is true if original is executed
    public bool CoroutineMoveNextPrefix(IEnumerator instance, ref bool result, ref bool state)
    {
        if (!_enumeratorInstances.Contains(instance)) return true;

        StaticLogger.Trace($"coroutine MoveNext: {instance.GetType().SaneFullName()}");

        if (!DoneFirstMoveNext.Contains(instance))
        {
            DoneFirstMoveNext.Add(instance);
            StaticLogger.Trace("first MoveNext");
            state = true;
            return true;
        }

        var current = ReverseInvoker.Invoke(i => i.Current, instance);

        // managed async operation?
        if (current is AsyncOperation op && AsyncOperationTracker.ManagedInstance(op))
        {
            var isDone = op.isDone;
            StaticLogger.Trace("coroutine MoveNext with AsyncOperation, operation is managed by unitas" +
                               $", running MoveNext: {isDone}");
            if (!isDone)
                result = true;
            state = isDone;
            return isDone;
        }

        if (MonoBehaviourController.PausedExecution)
        {
            if (current is null)
            {
                StaticLogger.Trace("paused execution while coroutine MoveNext, Current is null, not running MoveNext");
                result = true;
                return false;
            }

            // TODO: i can probably just manually check for CoreModule / UnityEngine since its not like there's many of this
            if (current.GetType().Assembly.GetName().Name.StartsWith("UnityEngine"))
            {
                StaticLogger.Trace("paused execution while coroutine MoveNext" +
                                   $", Current is type: {current.GetType().SaneFullName()}" +
                                   " unity type, not running MoveNext");
                result = true;
                return false;
            }

            state = true;
            return true;
        }

        if (MonoBehaviourController.PausedUpdate && current is null or WaitForEndOfFrame)
        {
            StaticLogger.Trace("paused update execution while coroutine MoveNext" +
                               ", Current is null / WaitForEndOfFrame, not running MoveNext");
            result = true;
            return false;
        }

        state = true;
        return true;
    }

    public void CoroutineMoveNextPostfix(IEnumerator instance, bool result, bool state)
    {
        if (!state || result) return;
        // result is false, coroutine is to be stopped
        _enumeratorRunningInstances.Remove(instance);
    }

    private static readonly UnityCoroutineManager CoroutineManager =
        ContainerStarter.Kernel.GetInstance<UnityCoroutineManager>();

    private static void NewCoroutinePostfix(MonoBehaviour __instance, IEnumerator __result)
    {
        CoroutineManager.NewCoroutineHandle(__instance, __result);
    }

    // ReSharper restore InconsistentNaming
}