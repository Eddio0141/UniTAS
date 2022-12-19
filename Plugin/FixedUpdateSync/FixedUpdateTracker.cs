using System;
using System.Collections.Generic;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.LegacySafeWrappers;
using UnityEngine;

namespace UniTASPlugin.FixedUpdateSync;

// ReSharper disable once ClassNeverInstantiated.Global
public class FixedUpdateTracker : IOnFixedUpdate, ISyncFixedUpdate, IOnUpdate
{
    private readonly List<SyncData> _onSyncCallbacks = new();
    private int _fixedUpdateIndex;
    private bool _invalidIndexCounter = true;

    private float _lastDeltaTime;
    private float _lastFixedDeltaTime;

    public void FixedUpdate()
    {
        // TODO remove hardcoded dependency
        if (TimeWrap.FrameTimeNotSet) return;

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
            _invalidIndexCounter = true;
            return;
        }

        if (Math.Abs(_lastDeltaTime - Time.deltaTime) > 0.00001 ||
            Math.Abs(_lastFixedDeltaTime - Time.fixedDeltaTime) > 0.00001)
        {
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
                    continue;
                }

                onSyncCallback.Callback.Invoke();
                _onSyncCallbacks.RemoveAt(i);
                i--;
            }
        }

        _fixedUpdateIndex++;
    }

    public void OnSync(Action callback, uint syncOffset = 0, ulong cycleOffset = 0)
    {
        _onSyncCallbacks.Add(new(callback, syncOffset, cycleOffset));
    }

    private struct SyncData
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