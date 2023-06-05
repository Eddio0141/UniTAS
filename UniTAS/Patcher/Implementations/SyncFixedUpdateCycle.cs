using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

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
    private readonly ILogger _logger;

    private readonly double _tolerance;

    public SyncFixedUpdateCycle(ITimeEnv timeEnv, ITimeWrapper timeWrapper, ILogger logger)
    {
        _timeEnv = timeEnv;
        _timeWrapper = timeWrapper;
        _logger = logger;

        _tolerance = _timeWrapper.IntFPSOnly ? 1.0 / int.MaxValue : float.Epsilon;
    }

    public void UpdateUnconditional()
    {
        if (_processingCallback != null)
        {
            // keeps setting until matches the target
            _logger.LogDebug(
                $"re-setting frame time, didn't match target, offset: {UpdateInvokeOffset.Offset}");
            SetFrameTime();
        }
        else if (_pendingCallback != null)
        {
            _pendingCallback.Callback();
            _logger.LogDebug("Invoking pending callback");
            _pendingCallback = null;

            if (_pendingSync.Count == 0 && _restoreFrametime != 0)
            {
                _logger.LogDebug($"Restoring frame time to {_restoreFrametime}");
                _timeEnv.FrameTime = _restoreFrametime;

                _restoreFrametime = 0;
            }
            else
            {
                ProcessQueue();
            }
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
        _logger.LogDebug($"Added on sync callback with invoke offset: {invokeOffset}");
        _pendingSync.Enqueue(new(callback, invokeOffset));
    }

    private void ProcessQueue()
    {
        if (_pendingSync.Count == 0)
            return;

        _logger.LogDebug("Processing new sync fixed update callback");
        _processingCallback = _pendingSync.Dequeue();

        _logger.LogDebug(
            $"Fixed delta time: {Time.fixedDeltaTime}, invoke offset: {_processingCallback.InvokeOffset}, update invoke offset: {UpdateInvokeOffset.Offset}");

        // check immediate return
        var actualSeconds = TargetSecondsAndActualSeconds().Item2;
        if (actualSeconds < _tolerance)
        {
            _logger.LogDebug("Immediate sync fixed update callback");
            _processingCallback.Callback();
            _processingCallback = null;
            ProcessQueue();
            return;
        }

        if (_restoreFrametime == 0f)
        {
            _restoreFrametime = _timeEnv.FrameTime;
            _logger.LogDebug($"Storing original frame time for later restore: {_restoreFrametime}");
        }

        SetFrameTime();
    }

    private double GetTargetSeconds()
    {
        // we are currently UpdateInvokeOffset.Offset ms in the update and we want to invoke at InvokeOffset ms
        // this means we need to first reach to the next Time.fixedDeltaTime ms (and i guess the future ft) and then add the invoke to happen at InvokeOffset ms
        var futureOffset = UpdateInvokeOffset.Offset + _timeEnv.FrameTime;
        futureOffset %= Time.fixedDeltaTime;
        var target = Time.fixedDeltaTime - futureOffset + _processingCallback.InvokeOffset;
        // var target = Time.fixedDeltaTime - Patcher.Shared.UpdateInvokeOffset.Offset + _processingCallback.InvokeOffset;
        return target % Time.fixedDeltaTime;
    }

    private Tuple<double, double> TargetSecondsAndActualSeconds()
    {
        var targetSeconds = _processingCallback.TimeLeftSet ? _processingCallback.TimeLeft : GetTargetSeconds();
        // unlike normal frame time, i round up
        var actualSeconds = _timeWrapper.IntFPSOnly ? 1.0 / (int)Math.Ceiling(1.0 / targetSeconds) : targetSeconds;
        return new(targetSeconds, actualSeconds);
    }

    private void SetFrameTime()
    {
        var seconds = TargetSecondsAndActualSeconds();
        var targetSeconds = seconds.Item1;
        _processingCallback.TimeLeft = targetSeconds;
        var actualSeconds = seconds.Item2;
        _logger.LogDebug($"Actual seconds: {actualSeconds}, target seconds: {targetSeconds}");

        _processingCallback.ProgressOffset(actualSeconds);

        _timeEnv.FrameTime = (float)actualSeconds;

        // check rounding
        if (targetSeconds - actualSeconds > _tolerance)
        {
            _logger.LogDebug(
                $"Didn't match target sync offset, difference: {targetSeconds - actualSeconds}, waiting for next frame");
        }
        else
        {
            _logger.LogDebug("Sync happening in next frame, pending restore frame time");
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