using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class SyncFixedUpdate : ISyncFixedUpdate, IOnUpdate, IOnPreUpdates
{
    private readonly Queue<SyncData> _pendingSync = new();
    private SyncData _pendingCallback;
    private SyncData _processingCallback;
    private double _restoreFrametime;

    private readonly ITimeEnv _timeEnv;
    private readonly ITimeWrapper _timeWrapper;

    public SyncFixedUpdate(ITimeEnv timeEnv, ITimeWrapper timeWrapper)
    {
        _timeEnv = timeEnv;
        _timeWrapper = timeWrapper;
    }

    public void PreUpdate()
    {
        if (_pendingCallback != null)
        {
            _pendingCallback.Callback();
            Trace.Write("Invoking pending callback");
            _pendingCallback = null;
            ProcessQueue();
        }

        if (_restoreFrametime == 0 || _processingCallback != null) return;

        Trace.Write($"Restoring frame time to {_restoreFrametime}");
        _timeEnv.FrameTime = _restoreFrametime;

        _restoreFrametime = 0;
    }

    public void Update()
    {
        if (_processingCallback == null) return;

        // keeps setting until matches the target
        Trace.Write("re-setting frame time, didn't match target");
        SetFrameTime();
    }

    public void OnSync(Action callback, double invokeOffset)
    {
        Trace.Write($"Added on sync callback with invoke offset: {invokeOffset}");
        _pendingSync.Enqueue(new(callback, invokeOffset));
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        if (_pendingSync.Count == 0)
            return;

        Trace.Write("Processing new callback");
        _processingCallback = _pendingSync.Dequeue();

        Trace.Write(
            $"Fixed delta time: {Time.fixedDeltaTime}, invoke offset: {_processingCallback.InvokeOffset}, update invoke offset: {Patcher.Shared.UpdateInvokeOffset.Offset}");

        // check immediate return
        // TODO idk what the tolerance should be but probably lower it later
        if (Math.Abs(GetTargetSeconds() - _processingCallback.InvokeOffset) < 0.0001)
        {
            Trace.Write("Immediate return callback");
            _processingCallback.Callback();
            _processingCallback = null;
            ProcessQueue();
            return;
        }

        if (_restoreFrametime == 0f)
        {
            _restoreFrametime = _timeEnv.FrameTime;
            Trace.Write($"Storing original frame time: {_restoreFrametime}");
        }

        SetFrameTime();
    }

    private double GetTargetSeconds()
    {
        return Time.fixedDeltaTime + _processingCallback.InvokeOffset - Patcher.Shared.UpdateInvokeOffset.Offset;
    }

    private void SetFrameTime()
    {
        var targetSeconds = GetTargetSeconds();
        // unlike normal frame time, i round down
        var actualSeconds = _timeWrapper.IntFPSOnly ? 1.0 / (int)(1.0 / targetSeconds) : targetSeconds;
        Trace.Write($"Actual seconds: {actualSeconds}");

        _timeEnv.FrameTime = (float)actualSeconds;

        // check rounding
        if (targetSeconds - actualSeconds > 0.0001)
        {
            Trace.Write(
                $"Actual seconds: {actualSeconds} Target seconds: {targetSeconds}, difference: {targetSeconds - actualSeconds}");
        }
        else
        {
            Trace.Write($"Pending restore frame time, target seconds is {targetSeconds}");
            _pendingCallback = _processingCallback;
            _processingCallback = null;
        }
    }

    private class SyncData
    {
        public Action Callback { get; }
        public double InvokeOffset { get; }

        public SyncData(Action callback, double invokeOffset)
        {
            Callback = callback;
            InvokeOffset = invokeOffset;
        }
    }
}