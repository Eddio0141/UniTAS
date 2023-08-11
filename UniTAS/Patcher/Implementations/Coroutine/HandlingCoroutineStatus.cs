using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Models.Coroutine;

namespace UniTAS.Patcher.Implementations.Coroutine;

/// <summary>
/// Class holding information about the currently handling coroutine
/// Unless you are making your own implementation of <see cref="CoroutineWait"/>, you should be using <see cref="CoroutineStatus"/>
/// </summary>
public class HandlingCoroutineStatus : IDisposable
{
    public CoroutineStatus CoroutineStatus { get; }
    public IEnumerator<CoroutineWait> Coroutine { get; }

    public HandlingCoroutineStatus(CoroutineStatus coroutineStatus, IEnumerable<CoroutineWait> coroutine)
    {
        CoroutineStatus = coroutineStatus;
        Coroutine = coroutine.GetEnumerator();
    }

    public void CoroutineFinish()
    {
        CoroutineStatus.IsRunning = false;
        Dispose();
    }

    public void Dispose()
    {
        Coroutine?.Dispose();
    }
}