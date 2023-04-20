using System;
using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations.Coroutine;

[Singleton]
public class CoroutineHandler : ICoroutine, IOnUpdateUnconditional, IOnPreUpdatesUnconditional
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
    private readonly Queue<Status> _waitForCoroutine = new();
    private readonly Queue<Status> _preUpdatesUnconditional = new();

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
            case WaitForPreUpdatesUnconditional:
                _preUpdatesUnconditional.Enqueue(status);
                break;
        }
    }

    public void UpdateUnconditional()
    {
        var count = _updateUnconditional.Count;
        while (count > 0)
        {
            count--;
            RunNext(_updateUnconditional.Dequeue());
        }
    }

    public void PreUpdateUnconditional()
    {
        ProcessWaitForCoroutine();
        ProcessPreUpdatesUnconditional();
    }

    private void ProcessPreUpdatesUnconditional()
    {
        var count = _preUpdatesUnconditional.Count;
        while (count > 0)
        {
            count--;
            RunNext(_preUpdatesUnconditional.Dequeue());
        }
    }

    private void ProcessWaitForCoroutine()
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
}