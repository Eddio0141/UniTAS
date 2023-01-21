using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.MonoBehCoroutineEndOfFrameTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class EndOfFrameTracker : IEndOfFrameTracker
{
    // the instance hash and status
    private readonly List<CoroutineTrackingStatus> _allStatus = new();

    // we alternate between those wait counters to avoid the same wait counter being used twice in the same frame
    private int _waitCount;
    private int _waitCount2;

    private readonly IOnLastUpdate[] _onLastUpdates;

    public EndOfFrameTracker(IOnLastUpdate[] onLastUpdates)
    {
        _onLastUpdates = onLastUpdates;
    }

    public void NewCoroutine(IEnumerator coroutine,
        object monoBehaviourInstance,
        string stringCoroutineMethodName = null)
    {
        Trace.Write($"New coroutine, {coroutine}");
        _allStatus.Add(new(coroutine, monoBehaviourInstance, stringCoroutineMethodName));
    }

    public void MoveNextInvoke(IEnumerator coroutine)
    {
        // the execution cycle is as follows
        // first something runs then yield return WaitForEndOfFrame or something else (or coroutine ends for some reason)
        // then when the next MoveNext is called, that is when the yield return stop is reached, then we can do our stuff
        // on that MoveNext, we also get the next yield return and the cycle repeats

        var coroutineHash = coroutine.GetHashCode();
        var trackingStatus = _allStatus.Find(x => x.CoroutineHash == coroutineHash);
        if (trackingStatus is null)
        {
            throw new InvalidOperationException("Coroutine not found, this should not happen");
        }

        // in case of the status already being true, that is when we are in the last update
        if (trackingStatus.IsWaitForEndOfFrame is true)
        {
            var usingCounter1 = trackingStatus.UsingWaitCounter1;
            if (usingCounter1)
            {
                _waitCount--;
            }
            else
            {
                _waitCount2--;
            }

            Trace.Write(
                $"MoveNext invoke, coroutine: {coroutine}, wait count: {_waitCount}, {_waitCount2}, using counter 1: {trackingStatus.UsingWaitCounter1}");
            trackingStatus.UsingWaitCounter1 = !usingCounter1;

            if (usingCounter1 && _waitCount == 0 || !usingCounter1 && _waitCount2 == 0)
            {
                Trace.Write("Invoking OnLastUpdate");
                foreach (var onLastUpdate in _onLastUpdates)
                {
                    onLastUpdate.OnLastUpdate();
                }
            }
        }

        // get new status
        var newIsWaitForEndOfFrame = coroutine.Current is WaitForEndOfFrame;
        trackingStatus.IsWaitForEndOfFrame = newIsWaitForEndOfFrame;
        if (newIsWaitForEndOfFrame)
        {
            if (trackingStatus.UsingWaitCounter1)
            {
                _waitCount++;
            }
            else
            {
                _waitCount2++;
            }

            Trace.Write(
                $"waiting for end of frame, coroutine: {coroutine}, wait count: {_waitCount}, {_waitCount2}, using counter 1: {trackingStatus.UsingWaitCounter1}");
        }
    }

    public void CoroutineEnd(IEnumerator coroutine)
    {
        var trackingStatus = _allStatus.Find(x => x.CoroutineHash == coroutine.GetHashCode());
        if (trackingStatus is null)
        {
            return;
        }

        Trace.Write(
            $"Coroutine end, {coroutine}, wait count: {_waitCount}, {_waitCount2}, using counter 1: {trackingStatus.UsingWaitCounter1}");

        _allStatus.Remove(trackingStatus);
    }

    public void CoroutineEnd(object monoBehaviourInstance, string coroutineMethodName)
    {
        var monoBehHash = monoBehaviourInstance.GetHashCode();
        var trackingStatuses = _allStatus.FindAll(x =>
            monoBehHash == x.MonoBehHash && x.StringCoroutineMethodName == coroutineMethodName);

        foreach (var trackingStatus in trackingStatuses)
        {
            Trace.Write(
                $"Coroutine end, {trackingStatus.CoroutineHash}, wait count: {_waitCount}, {_waitCount2}, using counter 1: {trackingStatus.UsingWaitCounter1}");

            _allStatus.Remove(trackingStatus);
        }
    }

    private class CoroutineTrackingStatus
    {
        public int CoroutineHash { get; }
        public int MonoBehHash { get; }
        public string StringCoroutineMethodName { get; }
        public bool? IsWaitForEndOfFrame { get; set; }
        public bool UsingWaitCounter1 { get; set; } = true;

        public CoroutineTrackingStatus(IEnumerator coroutine, object monoBeh, string stringCoroutineMethodName)
        {
            MonoBehHash = monoBeh.GetHashCode();
            CoroutineHash = coroutine.GetHashCode();
            StringCoroutineMethodName = stringCoroutineMethodName;
        }
    }
}