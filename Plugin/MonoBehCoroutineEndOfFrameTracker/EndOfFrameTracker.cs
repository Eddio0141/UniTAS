using System;
using System.Collections;
using System.Collections.Generic;
using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.MonoBehCoroutineEndOfFrameTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class EndOfFrameTracker : IEndOfFrameTracker
{
    // the instance hash and status
    private readonly Dictionary<int, CoroutineTrackingStatus> _allStatus = new();

    // we alternate between those wait counters to avoid the same wait counter being used twice in the same frame
    private int _waitCount;
    private int _waitCount2;

    private readonly IOnLastUpdate[] _onLastUpdates;

    public EndOfFrameTracker(IOnLastUpdate[] onLastUpdates)
    {
        _onLastUpdates = onLastUpdates;
    }

    public void NewCoroutine(IEnumerator coroutine)
    {
        _allStatus.Add(coroutine.GetHashCode(), new());
    }

    public void MoveNextInvoke(IEnumerator coroutine)
    {
        // the execution cycle is as follows
        // first something runs then yield return WaitForEndOfFrame or something else (or coroutine ends for some reason)
        // then when the next MoveNext is called, that is when the yield return stop is reached, then we can do our stuff
        // on that MoveNext, we also get the next yield return and the cycle repeats

        var coroutineHash = coroutine.GetHashCode();
        if (!_allStatus.TryGetValue(coroutineHash, out var trackingStatus))
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

            trackingStatus.UsingWaitCounter1 = !usingCounter1;

            if (usingCounter1 && _waitCount == 0 || !usingCounter1 && _waitCount2 == 0)
            {
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
        }
    }

    public void CoroutineEnd(IEnumerator coroutine)
    {
        _allStatus.Remove(coroutine.GetHashCode());
    }

    private class CoroutineTrackingStatus
    {
        public bool? IsWaitForEndOfFrame { get; set; }
        public bool UsingWaitCounter1 { get; set; } = true;
    }
}