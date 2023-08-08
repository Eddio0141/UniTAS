using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Models.Coroutine;

namespace UniTAS.Patcher.Implementations.Coroutine;

/// <summary>
/// Class holding information about the currently handling coroutine
/// Unless you are making your own implementation of <see cref="CoroutineWait"/>, you should be using <see cref="CoroutineStatus"/>
/// </summary>
public class HandlingCoroutineStatus
{
    public CoroutineStatus CoroutineStatus { get; }
    public IEnumerator<CoroutineWait> Coroutine { get; }

    public HandlingCoroutineStatus(CoroutineStatus coroutineStatus, IEnumerator<CoroutineWait> coroutine)
    {
        CoroutineStatus = coroutineStatus;
        Coroutine = coroutine;
    }
}