using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.LegacySafeWrappers;
using UnityEngine;

namespace UniTASPlugin.FixedUpdateSync;

// ReSharper disable once ClassNeverInstantiated.Global
public class SyncFixedUpdate : IOnFixedUpdate, ISyncFixedUpdate, IOnUpdate
{
    private readonly List<SyncData> _onSyncCallbacks = new();
    private int _fixedUpdateIndex;
    private bool _invalidIndexCounter = true;

    private float _lastDeltaTime;
    private float _lastFixedDeltaTime;

    public void FixedUpdate()
    {
        // TODO remove hardcoded dependency
        if (TimeWrap.FrameTimeNotSet)
        {
            Trace.WriteIf(_onSyncCallbacks.Count > 0, "Reached invalid counter, frametime not set 1");
            return;
        }

        _fixedUpdateIndex = 0;

        // TODO remove hardcoded dependency
        _lastFixedDeltaTime = Time.fixedDeltaTime;
        _invalidIndexCounter = false;
    }

    public void Update()
    {
        if (_invalidIndexCounter) return;

        // TODO remove hardcoded dependency
        // because this tracker works with fixed frame rate, we can't use the tracker unless the fixed frame rate is set
        if (TimeWrap.FrameTimeNotSet)
        {
            Trace.WriteIf(_onSyncCallbacks.Count > 0, "Reached invalid counter, frametime not set 2");
            _invalidIndexCounter = true;
            return;
        }

        if (Math.Abs(_lastDeltaTime - Time.deltaTime) > 0.00001 ||
            Math.Abs(_lastFixedDeltaTime - Time.fixedDeltaTime) > 0.00001)
        {
            Trace.WriteIf(_onSyncCallbacks.Count > 0,
                $"Reached invalid counter, skipping sync fixed update invoke, last delta time: {_lastDeltaTime}, delta time: {Time.deltaTime}, fixed delta time: {Time.fixedDeltaTime}");
            _invalidIndexCounter = true;
            _lastDeltaTime = Time.deltaTime;
            return;
        }

        // TODO remove hardcoded dependency
        var maxUpdateCount = (int)Math.Round(Time.fixedDeltaTime / Time.deltaTime);

        for (var i = 0; i < _onSyncCallbacks.Count; i++)
        {
            var onSyncCallback = _onSyncCallbacks[i];

            // remove on match
            if ((onSyncCallback.SyncOffset == 0 && _fixedUpdateIndex == 0) ||
                (onSyncCallback.SyncOffset != 0 && maxUpdateCount - onSyncCallback.SyncOffset == _fixedUpdateIndex))
            {
                if (onSyncCallback.CycleOffset > 0)
                {
                    onSyncCallback.CycleOffset--;
                    Trace.Write($"OnSyncCallback cycle offset > 0, count left: {onSyncCallback.CycleOffset}");
                    continue;
                }

                Trace.Write("OnSyncCallback cycle offset == 0, invoking");
                onSyncCallback.Callback.Invoke();
                _onSyncCallbacks.RemoveAt(i);
                i--;
            }
        }

        _fixedUpdateIndex++;
    }

    public void OnSync(Action callback, uint syncOffset = 0, ulong cycleOffset = 0)
    {
        Trace.Write($"Added on sync callback with sync offset: {syncOffset}, cycle offset: {cycleOffset}");
        _onSyncCallbacks.Add(new(callback, syncOffset, cycleOffset));
    }

    private class SyncData
    {
        public Action Callback { get; }
        public uint SyncOffset { get; }
        public ulong CycleOffset { get; set; }

        public SyncData(Action callback, uint syncOffset, ulong cycleOffset)
        {
            Callback = callback;
            SyncOffset = syncOffset;
            CycleOffset = cycleOffset;
        }
    }
}