using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations.Coroutine;

[Singleton]
public class CoroutineHandler : ICoroutine, IOnUpdateUnconditional
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

    public CoroutineStatus Start(IEnumerator<CoroutineWait> coroutine)
    {
        var status = new Status(new(), coroutine);
        RunNext(status);
        return status.CoroutineStatus;
    }

    private void RunNext(Status status)
    {
        var coroutine = status.Coroutine;
        if (!coroutine.MoveNext())
        {
            status.CoroutineStatus.IsRunning = false;
            return;
        }

        var current = coroutine.Current;
        if (current is WaitForUpdateUnconditional)
        {
            _updateUnconditional.Enqueue(status);
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
}