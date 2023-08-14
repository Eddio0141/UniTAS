using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForCoroutine : CoroutineWait
{
    private readonly IEnumerable<CoroutineWait> _coroutineMethod;

    public WaitForCoroutine(IEnumerable<CoroutineWait> coroutineMethod)
    {
        _coroutineMethod = coroutineMethod;
    }

    protected override void HandleWait()
    {
        var coroutineStatus = Container.GetInstance<ICoroutine>().Start(_coroutineMethod);
        coroutineStatus.OnComplete += _ => RunNext();
    }
}