using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityManagers;

[Singleton]
[ExcludeRegisterIfTesting]
public class UnityCoroutineManager : ICoroutineTracker, IOnPreGameRestart
{
    private readonly Dictionary<IEnumerator, MonoBehaviour> _instances = [];

    public void NewCoroutine(object instance, IEnumerator routine) =>
        NewCoroutineHandle(instance as MonoBehaviour, routine);

    public void NewCoroutine(MonoBehaviour instance, IEnumerator routine) => NewCoroutineHandle(instance, routine);

    private readonly Dictionary<Type, HashSet<MethodBase>> _patchedCoroutinesMethods = [];
    private readonly ILogger _logger;
    private readonly IHarmony _harmony;
    private readonly IUpdateEvents _updateEvents;

    public UnityCoroutineManager(ILogger logger, IHarmony harmony, IUpdateEvents updateEvents)
    {
        CoroutineManager = this;
        _logger = logger;
        _harmony = harmony;
        _updateEvents = updateEvents;
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

    private void NewCoroutineHandle(MonoBehaviour instance, IEnumerator routine)
    {
        if (instance == null || routine == null) return;
        // don't track ours
        var routineType = routine.GetType();
        if (Equals(routineType.Assembly, typeof(UnityCoroutineManager).Assembly)) return;

        _logger.LogDebug(
            $"new coroutine made in script {instance.GetType().SaneFullName()}, got IEnumerator {routineType.SaneFullName()}");
        StaticLogger.Trace($"call from {new StackTrace()}");

        _instances.Add(routine, instance);
    }

    public void OnPreGameRestart()
    {
        foreach (var coroutine in _instances.Values)
        {
            if (coroutine == null) continue;
            coroutine.StopAllCoroutines();
        }

        _instances.Clear();
        // no need, but better clean it up
        DoneFirstMoveNext.Clear();

        _startCall = false;
        _updateEvents.OnStartUnconditional += StartUnconditional;
    }

    private bool _startCall;

    private void StartUnconditional()
    {
        _updateEvents.OnStartUnconditional -= StartUnconditional;
        _startCall = true;
        _updateEvents.OnFixedUpdateUnconditional += FixedUpdateUnconditional;
    }

    private void FixedUpdateUnconditional()
    {
        _updateEvents.OnFixedUpdateUnconditional -= FixedUpdateUnconditional;
        _startCall = false;
    }

    private static readonly HarmonyMethod NewCoroutinePostfixMethod =
        new(typeof(UnityCoroutineManager), nameof(NewCoroutinePostfix));

    private static readonly IMonoBehaviourController MonoBehaviourController =
        ContainerStarter.Kernel.GetInstance<IMonoBehaviourController>();

    private static readonly IAsyncOperationTracker AsyncOperationTracker =
        ContainerStarter.Kernel.GetInstance<IAsyncOperationTracker>();

    private static readonly IPatchReverseInvoker ReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private class YieldNone : IEnumerator
    {
        public bool MoveNext() => false;

        public void Reset()
        {
        }

        public object Current => null;
    }

    private static readonly ITimeEnv TimeEnv = ContainerStarter.Kernel.GetInstance<ITimeEnv>();

    private class WaitForSecondsDummy(float seconds, bool startCall) : IEnumerator
    {
        private float _seconds = seconds;
        private bool _done;
        private bool _startCall = startCall;

        public bool MoveNext()
        {
            if (MonoBehaviourController.PausedExecution) return true;

            if (_startCall)
            {
                StaticLogger.Trace($"WaitForSeconds, startCall false, {GetHashCode()}");
                _startCall = false;
                return true;
            }

            if (_done)
            {
                StaticLogger.Trace($"WaitForSeconds: finish, {GetHashCode()}");
                return false;
            }

            _seconds -= (float)TimeEnv.FrameTime * Time.timeScale;
            if (_seconds <= 0)
            {
                StaticLogger.Trace($"WaitForSeconds: done, {GetHashCode()}");
                _done = true;
            }

            return true;
        }

        public void Reset()
        {
        }

        public object Current => null;
    }

    // private class WaitForSecondsRealTimeDummy(float seconds) : IEnumerator
    // {
    //     private float _seconds = seconds;
    //     private bool _done;
    //
    //     public bool MoveNext()
    //     {
    //         if (MonoBehaviourController.PausedExecution) return true;
    //         if (_done)
    //         {
    //             StaticLogger.Trace($"WaitForSecondsRealTime: done, {GetHashCode()}");
    //             return false;
    //         }
    //
    //         _seconds -= (float)TimeEnv.FrameTime;
    //         if (_seconds <= 0)
    //             _done = true;
    //
    //         return true;
    //     }
    //
    //     public void Reset()
    //     {
    //     }
    //
    //     public object Current => null;
    // }

    private static readonly YieldNone NoYield = new();

    private static readonly IMonoBehEventInvoker MonoBehEventInvoker =
        ContainerStarter.Kernel.GetInstance<IMonoBehEventInvoker>();

    private static readonly IAsyncOperationOverride AsyncOperationOverride =
        ContainerStarter.Kernel.GetInstance<IAsyncOperationOverride>();

    // ReSharper disable InconsistentNaming
    public void CoroutineCurrentPostfix(IEnumerator __instance, ref object __result)
    {
        if (!_instances.ContainsKey(__instance)) return;

        if (ReverseInvoker.Invoking || __result is null) return;

        StaticLogger.Trace($"coroutine get_Current: {__instance.GetType().SaneFullName()}, result: {__result}");

        // managed async operation?
        if (__result is AsyncOperation op && AsyncOperationOverride.Yield(op))
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

        if (__result is WaitForSeconds waitForSeconds)
        {
            __result = new WaitForSecondsDummy(waitForSeconds.m_Seconds, _startCall);
            StaticLogger.Trace(
                $"new WaitForSeconds with {waitForSeconds.m_Seconds} seconds, {__result.GetHashCode()}\n{Environment.StackTrace}");
            return;
        }

        // if (__result.GetType().SaneFullName() == "UnityEngine.WaitForSecondsRealtime")
        // {
        //     var waitTime = new Traverse(__result).Property("waitTime").GetValue<float>();
        //     __result = new WaitForSecondsRealTimeDummy(waitTime);
        //     StaticLogger.Trace(
        //         $"new WaitForSecondsRealTime with {waitTime} seconds, {__result.GetHashCode()}\n{Environment.StackTrace}");
        //     return;
        // }

        if (MonoBehaviourController.PausedExecution)
        {
            if (__result is WaitForEndOfFrame or WaitForFixedUpdate or null)
            {
                return;
            }

            // TODO: i can probably just manually check for CoreModule / UnityEngine since its not like there's many of this
            if (__result.GetType().Assembly.GetName().Name.StartsWith("UnityEngine"))
            {
                StaticLogger.Trace("paused execution for coroutine Current" +
                                   $", result is unity type: {__result.GetType().SaneFullName()}" +
                                   $", replaced result with null: {new StackTrace()}");
                __result = null;
            }

            // return;
        }
    }

    private static readonly HashSet<IEnumerator> DoneFirstMoveNext = [];

    // state is true if original is executed
    public bool CoroutineMoveNextPrefix(IEnumerator instance, ref bool result)
    {
        if (!_instances.ContainsKey(instance)) return true;

        StaticLogger.Trace($"coroutine MoveNext: {instance.GetType().SaneFullName()}");

        if (!DoneFirstMoveNext.Contains(instance))
        {
            DoneFirstMoveNext.Add(instance);
            StaticLogger.Trace("first MoveNext");
            return true;
        }

        var current = ReverseInvoker.Invoke(i => i.Current, instance);

        if (current is WaitForEndOfFrame)
        {
            MonoBehEventInvoker.InvokeEndOfFrame();
        }

        // managed async operation?
        if (current is AsyncOperation op && AsyncOperationTracker.ManagedInstance(op))
        {
            var isDone = op.isDone;
            StaticLogger.Trace("coroutine MoveNext with AsyncOperation, operation is managed by unitas" +
                               $", running MoveNext: {isDone}");
            if (!isDone)
                result = true;
            return isDone;
        }

        if (MonoBehaviourController.PausedExecution)
        {
            if (current is WaitForEndOfFrame or WaitForFixedUpdate or null)
            {
                StaticLogger.Trace(
                    $"paused execution while coroutine MoveNext, Current is {current}, not running MoveNext");
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

            return true;
        }

        return true;
    }

    private static UnityCoroutineManager CoroutineManager;

    private static void NewCoroutinePostfix(MonoBehaviour __instance, IEnumerator __result)
    {
        CoroutineManager.NewCoroutineHandle(__instance, __result);
    }

    // ReSharper restore InconsistentNaming
}