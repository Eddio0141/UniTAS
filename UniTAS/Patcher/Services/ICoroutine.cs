using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Models.Coroutine;

namespace UniTAS.Patcher.Services;

public interface ICoroutine
{
    /// <summary>
    /// Initializes the coroutine and starts running it
    /// </summary>
    /// <param name="coroutine"></param>
    /// <returns></returns>
    CoroutineStatus Start(IEnumerable<CoroutineWait> coroutine);
}