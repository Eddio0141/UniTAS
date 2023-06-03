using UniTAS.Patcher.Interfaces.Coroutine;

namespace UniTAS.Patcher.Models.Coroutine;

public class WaitForCoroutine : CoroutineWait
{
    public CoroutineStatus CoroutineStatus { get; }

    public WaitForCoroutine(CoroutineStatus coroutineStatus)
    {
        CoroutineStatus = coroutineStatus;
    }
}