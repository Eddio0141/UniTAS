using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Models.Coroutine;

namespace UniTAS.Patcher.Services;

public interface ICoroutine
{
    CoroutineStatus Start(IEnumerator<CoroutineWait> coroutine);
}