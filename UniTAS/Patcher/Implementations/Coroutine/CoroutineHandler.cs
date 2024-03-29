using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StructureMap;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Coroutine;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.Coroutine;

[Singleton(RegisterPriority.CoroutineHandler)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CoroutineHandler : ICoroutine, ICoroutineRunNext
{
    private readonly IContainer _container;

    public CoroutineHandler(IContainer container)
    {
        _container = container;
    }

    public CoroutineStatus Start(IEnumerable<CoroutineWait> coroutine)
    {
        var status = new HandlingCoroutineStatus(new(), coroutine);

        // null check
        if (coroutine == null)
        {
            status.CoroutineFinish();
        }
        else
        {
            RunNext(status);
        }

        return status.CoroutineStatus;
    }

    public void RunNext(HandlingCoroutineStatus handlingCoroutineStatus)
    {
        var coroutine = handlingCoroutineStatus.Coroutine;
        bool moveNext;
        try
        {
            moveNext = coroutine.MoveNext();
        }
        catch (Exception e)
        {
            handlingCoroutineStatus.CoroutineStatus.Exception = e;
            handlingCoroutineStatus.CoroutineFinish();
            return;
        }

        if (!moveNext)
        {
            handlingCoroutineStatus.CoroutineFinish();
            return;
        }

        coroutine.Current?.HandleWait(this, handlingCoroutineStatus, _container);
    }
}