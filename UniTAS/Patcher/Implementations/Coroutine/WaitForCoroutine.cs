using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForCoroutine(IEnumerable<CoroutineWait> coroutineMethod) : CoroutineWait
{
    protected override void HandleWait()
    {
        var coroutineStatus = Container.GetInstance<ICoroutine>().Start(coroutineMethod);
        coroutineStatus.OnComplete += status =>
        {
            if (status.Exception != null)
                Container.GetInstance<ILogger>().LogFatal($"coroutine exception: {status.Exception}");
            RunNext();
        };
    }
}