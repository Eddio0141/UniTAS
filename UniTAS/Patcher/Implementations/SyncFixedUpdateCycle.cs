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
// manually testing this so no need
[ExcludeRegisterIfTesting]
public class SyncFixedUpdateCycle : ISyncFixedUpdateCycle, IOnUpdateUnconditional, IOnFixedUpdateUnconditional
{
    private readonly Queue<SyncData> _pendingSync = new();
    private SyncData _pendingCallback;
    private uint _pendingFixedUpdateCount;
    private SyncData _processingCallback;
    private double _restoreFrametime;

    private readonly ITimeEnv _timeEnv;
    private readonly ITimeWrapper _timeWrapper;
    private readonly ILogger _logger;

    // increase in FixedUpdate, reset to 0 in Update
    private uint _fixedUpdateIndex;

    public SyncFixedUpdateCycle(ITimeEnv timeEnv, ITimeWrapper timeWrapper, ILogger logger)
    {
        _timeEnv = timeEnv;
        _timeWrapper = timeWrapper;
        _logger = logger;
    }

    public void FixedUpdateUnconditional()
    {
        _fixedUpdateIndex++;

        // is it for fixed update to handle this
        if (_pendingCallback == null || _pendingCallback.FixedUpdateIndex == 0) return;

        _pendingFixedUpdateCount--;
        if (_pendingFixedUpdateCount > 0) return;

        // syncing, invoke pending callback
        _logger.LogDebug("Invoking pending callback in fixed update");
        InvokePendingCallback();

        if (_pendingSync.Count == 0)
        {
            // no more jobs? restore ft
            RestoreFt();
        }
    }

    public void UpdateUnconditional()
    {
        _fixedUpdateIndex = 0;

        if (_processingCallback != null)
        {
            // keeps setting until matches the target
            _logger.LogDebug(
                $"re-setting frame time, didn't match target, offset: {UpdateInvokeOffset.Offset}");
            SetFrameTimeAndHandlePendingCallback();
        }
        else if (_pendingCallback != null)
        {
            // push this callback to FixedUpdate to handle
            if (_pendingCallback.FixedUpdateIndex != 0) return;

            _logger.LogDebug($"Invoking pending callback in update, offset: {UpdateInvokeOffset.Offset}");
            InvokePendingCallback();

            if (_pendingSync.Count == 0)
            {
                // we have finished all of our pending syncs, restore ft
                RestoreFt();
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

    public void OnSync(Action callback, double invokeOffset = 0, uint fixedUpdateIndex = 0)
    {
        // make the offset within range of 0..fixedDeltaTime
        var fixedDt = (double)Time.fixedDeltaTime;
        invokeOffset %= fixedDt;
        if (invokeOffset < 0.0)
            invokeOffset += fixedDt;
        _logger.LogDebug(
            $"Added on sync callback with invoke offset: {invokeOffset}, fixed update index: {fixedUpdateIndex}");
        _pendingSync.Enqueue(new(callback, invokeOffset, fixedUpdateIndex));
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
        // only applies if matching FixedUpdate index
        var callbackFixedUpdateIndex = _processingCallback.FixedUpdateIndex;

        var immediateReturnOffset =
            Math.Abs(_processingCallback.InvokeOffset - UpdateInvokeOffset.Offset) % Time.fixedDeltaTime;

        _logger.LogDebug(
            $"checking immediate return, current offset and invoke offset diff: {immediateReturnOffset}, callback index: {_fixedUpdateIndex}");

        if (immediateReturnOffset < _timeEnv.TimeTolerance && callbackFixedUpdateIndex >= _fixedUpdateIndex)
        {
            if (callbackFixedUpdateIndex == _fixedUpdateIndex)
            {
                _logger.LogDebug(
                    $"Immediate sync fixed update callback, sync fixed update index: {callbackFixedUpdateIndex}, current: {_fixedUpdateIndex}");
                _processingCallback.Callback();
                _processingCallback = null;
                ProcessQueue();
            }
            else
            {
                _logger.LogDebug(
                    $"Callback index is not equals so waiting for fixed update to match {callbackFixedUpdateIndex}, current: {_fixedUpdateIndex}");
                // wait for fixed updates to do its thing
                SwitchToPendingCallback();
            }

            return;
        }

        if (_restoreFrametime == 0.0)
        {
            _restoreFrametime = _timeEnv.FrameTime;
            _logger.LogDebug($"Storing original frame time for later restore: {_restoreFrametime}");
        }

        SetFrameTimeAndHandlePendingCallback();
    }

    private double GetTargetSeconds()
    {
        // how long until next fixed update + the offset to be reached
        var target = _processingCallback.InvokeOffset - UpdateInvokeOffset.Offset;
        if (target < 0.0)
            target += Time.fixedDeltaTime;
        return target;
    }

    private Utils.Tuple<double, double> TargetSecondsAndActualSeconds(SyncData syncData)
    {
        var targetSeconds = syncData.TimeLeftSet ? syncData.TimeLeft : GetTargetSeconds();
        // unlike normal frame time, i round up
        var actualSeconds = _timeWrapper.IntFPSOnly ? 1.0 / (int)Math.Ceiling(1.0 / targetSeconds) : targetSeconds;
        return new(targetSeconds, actualSeconds);
    }

    private void SetFrameTimeAndHandlePendingCallback()
    {
        var seconds = TargetSecondsAndActualSeconds(_processingCallback);
        var targetSeconds = seconds.Item1;
        _processingCallback.TimeLeft = targetSeconds;
        var actualSeconds = seconds.Item2;
        _logger.LogDebug($"Actual seconds: {actualSeconds}, target seconds: {targetSeconds}");

        _processingCallback.ProgressOffset(actualSeconds);

        _timeEnv.FrameTime = (float)actualSeconds;

        // check rounding
        if (IsSyncHappening(targetSeconds, actualSeconds))
        {
            _logger.LogDebug(
                $"Didn't match target sync offset, difference: {targetSeconds - actualSeconds}, waiting for next frame");
        }
        else
        {
            _logger.LogDebug("Sync happening in next update, pending restore frame time");
            SwitchToPendingCallback();
        }
    }

    private bool IsSyncHappening(double targetSeconds, double actualSeconds)
    {
        return targetSeconds - actualSeconds > _timeEnv.TimeTolerance;
    }

    private void SwitchToPendingCallback()
    {
        _pendingCallback = _processingCallback;
        _processingCallback = null;
        // in case of fixed update callbacks, the ft is already set so we just need to wait for fixed updates
        _pendingFixedUpdateCount = _pendingCallback.FixedUpdateIndex;
    }

    private void InvokePendingCallback()
    {
        _pendingCallback.Callback();
        _pendingCallback = null;
    }

    private void RestoreFt()
    {
        _logger.LogDebug($"Restoring frame time to {_restoreFrametime}");
        _timeEnv.FrameTime = _restoreFrametime;
        _restoreFrametime = 0;
    }

    private class SyncData
    {
        private double _timeLeft;
        public Action Callback { get; }
        public double InvokeOffset { get; }
        public uint FixedUpdateIndex { get; }

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

        public SyncData(Action callback, double invokeOffset, uint fixedUpdateIndex)
        {
            Callback = callback;
            InvokeOffset = invokeOffset;
            FixedUpdateIndex = fixedUpdateIndex;
        }

        public void ProgressOffset(double offset) => _timeLeft -= offset;
    }
}