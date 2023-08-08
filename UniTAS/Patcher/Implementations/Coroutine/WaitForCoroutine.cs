using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Models.Coroutine;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForCoroutine : CoroutineWait
{
    private readonly CoroutineStatus _coroutineStatus;

    public WaitForCoroutine(CoroutineStatus coroutineStatus)
    {
        _coroutineStatus = coroutineStatus;
    }

    protected override void HandleWait()
    {
        _coroutineStatus.OnComplete += _ => RunNext();
    }
}