using StructureMap;
using UniTAS.Patcher.Implementations.Coroutine;

namespace UniTAS.Patcher.Interfaces.Coroutine;

public abstract class CoroutineWait
{
    private ICoroutineRunNext _coroutineRunNext;
    private HandlingCoroutineStatus _status;

    protected IContainer Container { get; private set; }

    public void HandleWait(ICoroutineRunNext coroutineRunNext, HandlingCoroutineStatus status, IContainer container)
    {
        _coroutineRunNext = coroutineRunNext;
        _status = status;
        Container = container;

        HandleWait();
    }

    protected virtual void HandleWait()
    {
    }

    /// <summary>
    /// Invoke this to make the coroutine continue running from this yield
    /// </summary>
    protected void RunNext()
    {
        _coroutineRunNext.RunNext(_status);
    }
}