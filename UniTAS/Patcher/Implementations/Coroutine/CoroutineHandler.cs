using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.Coroutine;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.Coroutine;

[Singleton(RegisterPriority.CoroutineHandler)]
public class CoroutineHandler : ICoroutine, IOnUpdateUnconditional, IOnPreUpdatesUnconditional,
    IOnLastUpdateUnconditional, IOnFixedUpdateUnconditional
{
    private class Status
    {
        public CoroutineStatus CoroutineStatus { get; }
        public IEnumerator<CoroutineWait> Coroutine { get; }

        public Status(CoroutineStatus coroutineStatus, IEnumerator<CoroutineWait> coroutine)
        {
            CoroutineStatus = coroutineStatus;
            Coroutine = coroutine;
        }
    }

    private readonly Queue<Status> _updateUnconditional = new();
    private readonly Queue<Status> _fixedUpdateUnconditional = new();
    private readonly Queue<Status> _waitForCoroutine = new();
    private readonly Queue<Status> _lastUpdateUnconditional = new();

    private readonly ISyncFixedUpdateCycle _syncFixedUpdateCycle;

    public CoroutineHandler(ISyncFixedUpdateCycle syncFixedUpdateCycle)
    {
        _syncFixedUpdateCycle = syncFixedUpdateCycle;
    }

    public CoroutineStatus Start(IEnumerator<CoroutineWait> coroutine)
    {
        var status = new Status(new(), coroutine);
        RunNext(status);
        return status.CoroutineStatus;
    }

    private void RunNext(Status status)
    {
        var coroutine = status.Coroutine;
        bool moveNext;
        try
        {
            moveNext = coroutine.MoveNext();
        }
        catch (Exception e)
        {
            status.CoroutineStatus.Exception = e;
            status.CoroutineStatus.IsRunning = false;
            return;
        }

        if (!moveNext)
        {
            status.CoroutineStatus.IsRunning = false;
            return;
        }

        switch (coroutine.Current)
        {
            case WaitForUpdateUnconditional:
                _updateUnconditional.Enqueue(status);
                break;
            case WaitForCoroutine:
                _waitForCoroutine.Enqueue(status);
                break;
            case WaitForLastUpdateUnconditional:
                _lastUpdateUnconditional.Enqueue(status);
                break;
            case WaitForFixedUpdateUnconditional:
                _fixedUpdateUnconditional.Enqueue(status);
                break;
            case WaitForOnSync waitForOnSync:
                _syncFixedUpdateCycle.OnSync(() => { RunNext(status); },
                    waitForOnSync.InvokeOffset, waitForOnSync.FixedUpdateIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(coroutine), "Unknown coroutine wait type");
        }
    }

    public void UpdateUnconditional()
    {
        var count = _updateUnconditional.Count;
        while (count > 0)
        {
            RunNext(_updateUnconditional.Dequeue());
            count--;
        }
    }

    public void PreUpdateUnconditional()
    {
        var count = _waitForCoroutine.Count;
        while (count > 0)
        {
            count--;

            var status = _waitForCoroutine.Dequeue();
            var current = (WaitForCoroutine)status.Coroutine.Current;
            if (current == null)
            {
                RunNext(status);
                continue;
            }

            if (!current.CoroutineStatus.IsRunning)
            {
                RunNext(status);
                continue;
            }

            _waitForCoroutine.Enqueue(status);
        }
    }

    public void OnLastUpdateUnconditional()
    {
        var count = _lastUpdateUnconditional.Count;
        while (count > 0)
        {
            RunNext(_lastUpdateUnconditional.Dequeue());
            count--;
        }
    }

    public void FixedUpdateUnconditional()
    {
        var count = _fixedUpdateUnconditional.Count;
        while (count > 0)
        {
            RunNext(_fixedUpdateUnconditional.Dequeue());
            count--;
        }
    }
}