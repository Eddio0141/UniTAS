using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class SyncFixedUpdate : IOnFixedUpdate, ISyncFixedUpdate, IOnUpdate
{
    private readonly List<SyncData> _onSyncCallbacks = new();
    private int _fixedUpdateIndex;
    private bool _invalidIndexCounter = true;

    private float _lastDeltaTime;
    private float _lastFixedDeltaTime;

    private readonly ITimeWrapper _timeWrap;

    public SyncFixedUpdate(ITimeWrapper timeWrap)
    {
        _timeWrap = timeWrap;
    }

    public void FixedUpdate()
    {
        Trace.WriteIf(_onSyncCallbacks.Count > 0, $"on sync callback count: {_onSyncCallbacks.Count}");
        if (_timeWrap.CaptureFrameTime == 0)
        {
            Trace.WriteIf(_onSyncCallbacks.Count > 0, "Reached invalid counter, frame-time not set 1");
            return;
        }

        _fixedUpdateIndex = 0;

        _lastFixedDeltaTime = Time.fixedDeltaTime;
        _invalidIndexCounter = false;
    }

    public void Update()
    {
        // Trace.Write($"Callback count, {_onSyncCallbacks.Count}");
        if (_invalidIndexCounter) return;

        // because this tracker works with fixed frame rate, we can't use the tracker unless the fixed frame rate is set
        if (_timeWrap.CaptureFrameTime == 0)
        {
            Trace.WriteIf(_onSyncCallbacks.Count > 0, "Reached invalid counter, frame-time not set 2");
            _invalidIndexCounter = true;
            return;
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (_lastDeltaTime != Time.deltaTime || _lastFixedDeltaTime != Time.fixedDeltaTime)
            // ReSharper restore CompareOfFloatsByEqualityOperator
        {
            Trace.WriteIf(_onSyncCallbacks.Count > 0,
                $"Reached invalid counter, skipping sync fixed update invoke, last delta time: {_lastDeltaTime}, delta time: {Time.deltaTime}, fixed delta time: {Time.fixedDeltaTime}");
            _invalidIndexCounter = true;
            _lastDeltaTime = Time.deltaTime;
            return;
        }

        var maxUpdateCount = (int)Math.Round(Time.fixedDeltaTime / Time.deltaTime);

        Trace.WriteIf(_onSyncCallbacks.Count > 0,
            $"Max update count: {maxUpdateCount}, fixed delta time: {Time.fixedDeltaTime}, delta time: {Time.deltaTime}, callbacks left: {_onSyncCallbacks.Count}");

        for (var i = 0; i < _onSyncCallbacks.Count; i++)
        {
            var onSyncCallback = _onSyncCallbacks[i];

            // remove on match
            if ((onSyncCallback.InvokeOffset == 0 && _fixedUpdateIndex == 0) ||
                (onSyncCallback.InvokeOffset != 0 && maxUpdateCount - onSyncCallback.InvokeOffset == _fixedUpdateIndex))
            {
                if (onSyncCallback.CycleOffset > 0)
                {
                    onSyncCallback.CycleOffset--;
                    Trace.Write($"OnSyncCallback cycle offset > 0, count left: {onSyncCallback.CycleOffset}");
                    continue;
                }

                Trace.Write(
                    $"OnSyncCallback cycle offset == 0, invoking at frame count {Time.frameCount}, sync offset: {onSyncCallback.InvokeOffset}, cycle offset: {onSyncCallback.CycleOffset}");
                onSyncCallback.Callback.Invoke();
                _onSyncCallbacks.RemoveAt(i);
                i--;
            }
        }

        _fixedUpdateIndex++;
    }

    public void OnSync(Action callback, double invokeOffset)
    {
        Trace.Write($"Added on sync callback with invoke offset: {invokeOffset}");
        _onSyncCallbacks.Add(new(callback, invokeOffset));
        Trace.Write($"Total on sync callback count: {_onSyncCallbacks.Count}");
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