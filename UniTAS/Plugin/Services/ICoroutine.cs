using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Models.Coroutine;

namespace UniTAS.Plugin.Services;

public interface ICoroutine
{
    CoroutineStatus Start(IEnumerator<CoroutineWait> coroutine);
}