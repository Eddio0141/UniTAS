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
    private readonly Queue<IEnumerator<CoroutineWait>> _updateUnconditional = new();

    public void Start(IEnumerator<CoroutineWait> coroutine)
    {
        RunNext(coroutine);
    }

    private void RunNext(IEnumerator<CoroutineWait> coroutine)
    {
        if (!coroutine.MoveNext()) return;

        var current = coroutine.Current;
        if (current is WaitForUpdateUnconditional)
        {
            _updateUnconditional.Enqueue(coroutine);
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