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
public class SyncFixedUpdateCycle : ISyncFixedUpdateCycle, IOnUpdateUnconditional, IOnFixedUpdateUnconditional
{
    private readonly Queue<SyncData> _pendingSync = new();
    private SyncData _pendingCallback;
    private SyncData _processingCallback;
    private double _restoreFrametime;

    private readonly ITimeEnv _timeEnv;
    private readonly ITimeWrapper _timeWrapper;
    private readonly ILogger _logger;
    private readonly IMonoBehaviourController _monoBehaviourController;

    private readonly double _tolerance;

    // increase in FixedUpdate, reset to 0 in Update
    private uint _fixedUpdateIndex;

    public SyncFixedUpdateCycle(ITimeEnv timeEnv, ITimeWrapper timeWrapper, ILogger logger,
        IMonoBehaviourController monoBehaviourController)
    {
        _timeEnv = timeEnv;
        _timeWrapper = timeWrapper;
        _logger = logger;
        _monoBehaviourController = monoBehaviourController;

        _tolerance = _timeWrapper.IntFPSOnly ? 1.0 / int.MaxValue : float.Epsilon;
    }

    /* probably an obvious issue but setting unity ft to something that won't cause it to have more than
       1 FixedUpdate and target fixed update index to > 1 is going to wait for sync forever
       
       probably won't handle cuz whoever uses this needs to do it properly lmao
     */
    public void FixedUpdateUnconditional()
    {
        if (!_monoBehaviourController.PausedExecution)
        {
            _fixedUpdateIndex++;
        }

        // only check if time left and index match
        if (_pendingCallback == null || _pendingCallback.FixedUpdateIndex != _fixedUpdateIndex) return;

        var seconds = TargetSecondsAndActualSeconds(_pendingCallback);
        var targetSeconds = seconds.Item1;
        var actualSeconds = seconds.Item2;

        if (!IsSyncHappening(targetSeconds, actualSeconds)) return;

        // syncing, invoke pending callback
        InvokePendingCallback();

        if (_pendingSync.Count == 0)
        {
            // no more jobs? restore ft
            RestoreFt();
        }
    }

    public void UpdateUnconditional()
    {
        if (!_monoBehaviourController.PausedExecution && !_monoBehaviourController.PausedUpdate)
        {
            _fixedUpdateIndex = 0;
        }

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

    public void OnSync(Action callback, double invokeOffset, uint fixedUpdateIndex = 0)
    {
        // make the offset within range of 0..fixedDeltaTime
        invokeOffset %= Time.fixedDeltaTime;
        if (invokeOffset < 0)
            invokeOffset += Time.fixedDeltaTime;
        _logger.LogDebug($"Added on sync callback with invoke offset: {invokeOffset}");
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
        var actualSeconds = TargetSecondsAndActualSeconds(_processingCallback).Item2;
        if (actualSeconds < _tolerance && _processingCallback.FixedUpdateIndex == _fixedUpdateIndex)
        {
            _logger.LogDebug(
                $"Immediate sync fixed update callback, sync fixed update index: {_processingCallback.FixedUpdateIndex}, current: {_fixedUpdateIndex}");
            if (_processingCallback.FixedUpdateIndex == _fixedUpdateIndex)
            {
                _processingCallback.Callback();
                _processingCallback = null;
                ProcessQueue();
            }
            else
            {
                // wait for fixed updates to do its thing
                SwitchToPendingCallback();
            }

            return;
        }

        if (_restoreFrametime == 0f)
        {
            _restoreFrametime = _timeEnv.FrameTime;
            _logger.LogDebug($"Storing original frame time for later restore: {_restoreFrametime}");
        }

        SetFrameTimeAndHandlePendingCallback();
    }

    private double GetTargetSeconds()
    {
        // we are currently UpdateInvokeOffset.Offset ms in the update and we want to invoke at InvokeOffset ms
        // this means we need to first reach to the next Time.fixedDeltaTime ms (and i guess the future ft) and then add the invoke to happen at InvokeOffset ms
        var futureOffset = UpdateInvokeOffset.Offset + _timeEnv.FrameTime;
        futureOffset %= Time.fixedDeltaTime;
        var target = Time.fixedDeltaTime - futureOffset + _processingCallback.InvokeOffset;
        return target % Time.fixedDeltaTime;
    }

    private Tuple<double, double> TargetSecondsAndActualSeconds(SyncData syncData)
    {
        var targetSeconds = syncData.TimeLeftSet ? syncData.TimeLeft : GetTargetSeconds();
        // unlike normal frame time, i round up
        var actualSeconds = _timeWrapper.IntFPSOnly ? 1.0 / (int)Math.Ceiling(1.0 / targetSeconds) : targetSeconds;
        return new(targetSeconds, actualSeconds);
    }

    // only use in Update!
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
            _logger.LogDebug("Sync happening in next frame, pending restore frame time");
            SwitchToPendingCallback();
        }
    }

    private bool IsSyncHappening(double targetSeconds, double actualSeconds)
    {
        return targetSeconds - actualSeconds > _tolerance;
    }

    private void SwitchToPendingCallback()
    {
        _pendingCallback = _processingCallback;
        _processingCallback = null;
    }

    private void InvokePendingCallback()
    {
        _logger.LogDebug("Invoking pending callback");
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