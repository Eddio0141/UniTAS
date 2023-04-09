using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class SyncFixedUpdateCycle : ISyncFixedUpdateCycle, IOnUpdateUnconditional
{
    private readonly Queue<SyncData> _pendingSync = new();
    private SyncData _pendingCallback;
    private SyncData _processingCallback;
    private double _restoreFrametime;

    private readonly ITimeEnv _timeEnv;
    private readonly ITimeWrapper _timeWrapper;

    private readonly double _tolerance;

    public SyncFixedUpdateCycle(ITimeEnv timeEnv, ITimeWrapper timeWrapper)
    {
        _timeEnv = timeEnv;
        _timeWrapper = timeWrapper;

        _tolerance = _timeWrapper.IntFPSOnly ? 1.0 / int.MaxValue : float.Epsilon;
    }

    public void UpdateUnconditional()
    {
        if (_processingCallback != null)
        {
            // keeps setting until matches the target
            Trace.Write(
                $"re-setting frame time, didn't match target, offset: {Patcher.Shared.UpdateInvokeOffset.Offset}");
            SetFrameTime();
        }
        else if (_pendingCallback != null)
        {
            _pendingCallback.Callback();
            Trace.Write("Invoking pending callback");
            _pendingCallback = null;
            ProcessQueue();
        }
        else if (_restoreFrametime != 0)
        {
            Trace.Write($"Restoring frame time to {_restoreFrametime}");
            _timeEnv.FrameTime = _restoreFrametime;

            _restoreFrametime = 0;
        }
        else
        {
            ProcessQueue();
        }
    }

    public void OnSync(Action callback, double invokeOffset)
    {
        // make the offset within range of 0..fixedDeltaTime
        invokeOffset %= Time.fixedDeltaTime;
        if (invokeOffset < 0)
            invokeOffset += Time.fixedDeltaTime;
        Trace.Write($"Added on sync callback with invoke offset: {invokeOffset}");
        _pendingSync.Enqueue(new(callback, invokeOffset));
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
        if (Math.Abs(Patcher.Shared.UpdateInvokeOffset.Offset - _processingCallback.InvokeOffset) < _tolerance)
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
        // we are currently UpdateInvokeOffset.Offset ms in the update and we want to invoke at InvokeOffset ms
        // this means we need to first reach to the next Time.fixedDeltaTime ms (and i guess the future ft) and then add the invoke to happen at InvokeOffset ms
        var futureOffset = Patcher.Shared.UpdateInvokeOffset.Offset + _timeEnv.FrameTime;
        futureOffset %= Time.fixedDeltaTime;
        var target = Time.fixedDeltaTime - futureOffset + _processingCallback.InvokeOffset;
        // var target = Time.fixedDeltaTime - Patcher.Shared.UpdateInvokeOffset.Offset + _processingCallback.InvokeOffset;
        return target % Time.fixedDeltaTime;
    }

    private void SetFrameTime()
    {
        var targetSeconds = _processingCallback.TimeLeftSet ? _processingCallback.TimeLeft : GetTargetSeconds();
        _processingCallback.TimeLeft = targetSeconds;
        // unlike normal frame time, i round up
        var actualSeconds = _timeWrapper.IntFPSOnly ? 1.0 / (int)Math.Ceiling(1.0 / targetSeconds) : targetSeconds;
        Trace.Write($"Actual seconds: {actualSeconds}, target seconds: {targetSeconds}");

        _processingCallback.ProgressOffset(actualSeconds);

        _timeEnv.FrameTime = (float)actualSeconds;

        // check rounding
        if (targetSeconds - actualSeconds > _tolerance)
        {
            Trace.Write(
                $"Actual seconds: {actualSeconds} Target seconds: {targetSeconds}, difference: {targetSeconds - actualSeconds}");
        }
        else
        {
            Trace.Write($"Pending restore frame time");
            _pendingCallback = _processingCallback;
            _processingCallback = null;
        }
    }

    private class SyncData
    {
        private double _timeLeft;
        public Action Callback { get; }
        public double InvokeOffset { get; }

        public double TimeLeft
        {
            get => _timeLeft;
            set
            {
                if (TimeLeftSet) return;
                TimeLeftSet = true;
                _timeLeft = value;
            }
        }

        public bool TimeLeftSet { get; private set; }

        public SyncData(Action callback, double invokeOffset)
        {
            Callback = callback;
            InvokeOffset = invokeOffset;
        }

        public void ProgressOffset(double offset) => _timeLeft -= offset;
    }
}