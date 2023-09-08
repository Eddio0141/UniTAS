using UniTAS.Patcher.Implementations.Coroutine;

namespace UniTAS.Patcher.Interfaces.Coroutine;

public interface ICoroutineRunNext
{
    /// <summary>
    /// Resumes coroutine until it ends or hits another yield
    /// </summary>
    void RunNext(HandlingCoroutineStatus handlingCoroutineStatus);
}