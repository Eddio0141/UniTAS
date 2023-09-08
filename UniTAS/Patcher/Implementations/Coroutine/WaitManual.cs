using UniTAS.Patcher.Interfaces.Coroutine;

namespace UniTAS.Patcher.Implementations.Coroutine;

/// <summary>
/// Invoke <see cref="RunNext"/> which will advance the coroutine
/// </summary>
public class WaitManual : CoroutineWait
{
    public new void RunNext()
    {
        base.RunNext();
    }
}