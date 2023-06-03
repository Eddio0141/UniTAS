using UniTAS.Plugin.Interfaces.Coroutine;

namespace UniTAS.Plugin.Models.Coroutine;

public class WaitForCoroutine : CoroutineWait
{
    public CoroutineStatus CoroutineStatus { get; }

    public WaitForCoroutine(CoroutineStatus coroutineStatus)
    {
        CoroutineStatus = coroutineStatus;
    }
}