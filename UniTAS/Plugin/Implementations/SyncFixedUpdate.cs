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
    private readonly Queue<SyncData> _pendingCallbacks = new();
    private SyncData _processingCallback;
    private float _restoreFrametime;
    private bool _pendingRestoreFrametime;

    private readonly ITimeEnv _timeEnv;
    private readonly ITimeWrapper _timeWrapper;

    public SyncFixedUpdate(ITimeEnv timeEnv, ITimeWrapper timeWrapper)
    {
        _timeEnv = timeEnv;
        _timeWrapper = timeWrapper;
    }

    public void PreUpdate()
    {
        if (!_pendingRestoreFrametime) return;
        _processingCallback.Callback();
    }

    public void Update()
    {
        if (_restoreFrametime == 0 && !_pendingRestoreFrametime)
            return;

        if (_pendingRestoreFrametime)
        {
            _timeEnv.FrameTime = _restoreFrametime;
            _pendingRestoreFrametime = false;
            _restoreFrametime = 0;
            ProcessQueue();
            return;
        }

        // keeps setting until matches the target
        SetFrameTime();
    }

    public void OnSync(Action callback, double invokeOffset)
    {
        Trace.Write($"Added on sync callback with invoke offset: {invokeOffset}");
        _pendingCallbacks.Enqueue(new(callback, invokeOffset));
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        if (_pendingCallbacks.Count == 0)
            return;

        _processingCallback = _pendingCallbacks.Dequeue();

        // check immediate return
        if (Math.Abs(GetTargetSeconds() - _processingCallback.InvokeOffset) < 0.0001)
        {
            Trace.Write("Immediate return");
            _processingCallback.Callback();
            ProcessQueue();
            return;
        }

        _restoreFrametime = _timeEnv.FrameTime;

        SetFrameTime();
    }

    private double GetTargetSeconds()
    {
        return Time.fixedDeltaTime + _processingCallback.InvokeOffset - Patcher.Shared.UpdateInvokeOffset.Offset;
    }

    private void SetFrameTime()
    {
        var targetSeconds = GetTargetSeconds();

        _timeEnv.FrameTime = (float)targetSeconds;

        // check rounding
        var actualSeconds = _timeWrapper.CaptureFrameTime;
        if (targetSeconds - actualSeconds > 0.0001)
        {
            Trace.Write(
                $"Actual seconds: {actualSeconds} Target seconds: {targetSeconds}, difference: {targetSeconds - actualSeconds}");

            _pendingRestoreFrametime = false;
        }
        else
        {
            _pendingRestoreFrametime = true;
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