using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;

namespace UniTAS.Plugin.Services;

public interface ICoroutine
{
    void Start(IEnumerator<CoroutineWait> coroutine);
}